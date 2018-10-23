using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RM.Lib.Common.Contracts.Log;
using RM.Lib.Proxy.Contracts;
using RM.Lib.Utility;
using RM.Lib.UzTicket.Model;
using RM.UzTicket.Lib.Exceptions;
using RM.UzTicket.Settings.Contracts;

namespace RM.Lib.UzTicket
{
	internal sealed class UzScanner : IDisposable
	{
		private class ScanData
		{
			private const int _lockTimeout = 100;
			private readonly object _lock;

			public ScanData(ScanItem item)
			{
				Item = item;
				Attempts = 0;

				_lock = new object();
			}

			public ScanItem Item { get; }

			public int Attempts { get; private set; }

			public string Error { get; private set; }

			public AsyncLock GetLock()
			{
				return AsyncLock.Lock(_lock, _lockTimeout);
			}

			public void IncAttempts()
			{
				Attempts++;
			}

			public void SetError(string error)
			{
				Error = error;
			}
		}

		private const int _initialDelay = 1 * 60;
		private const int _defaultDelay = 10 * 60;

		private readonly IUzSettings _settings;
		private readonly IProxyProvider _proxyProvider;
		private readonly ILog _log;
		private readonly int _delay;
		private readonly IDictionary<string, ScanData> _scanStates;
		private readonly CancellationTokenSource _cancelTokenSource;
		
		private volatile bool _isRunning;
		private bool _isDisposed;

		public UzScanner(IUzSettings settings, IProxyProvider proxyProvider, ILog log)
		{
			_settings = settings;
			_proxyProvider = proxyProvider;
			_log = log;

			_delay = _settings.ScanDelay * 60 ?? _defaultDelay;

			_scanStates = new ConcurrentDictionary<string, ScanData>();
			_cancelTokenSource = new CancellationTokenSource();
		}

		public event EventHandler<ScanEventArgs> ScanEvent;
		
		public void Dispose()
		{
			if (!_isDisposed)
			{
				Reset();
				_cancelTokenSource.Dispose();
				

				_isDisposed = true;
			}
		}

		public string AddItem(ScanItem item)
		{
			var scanId = Guid.NewGuid().ToString("N").ToUpperInvariant();

			_scanStates.Add(scanId, new ScanData(item));

			//if (!_isRunning)
			//{
			//	Start();
			//}

			return scanId;
		}

		public Tuple<int, string> GetStatus(string scanId)
		{
			if (_scanStates.TryGetValue(scanId, out var data))
			{
				return Tuple.Create(data.Attempts, data.Error);
			}

			throw new ScanNotFoundException(scanId);
		}

		public void Abort(string scanId)
		{
			if (!_scanStates.ContainsKey(scanId))
			{
				throw new ScanNotFoundException(scanId);
			}

			_scanStates.Remove(scanId);

			if (!_scanStates.Any())
			{
				Stop();
			}
		}

		public void Reset()
		{
			_isRunning = false;
			_cancelTokenSource.Cancel();
			_scanStates.Clear();
		}

		public void Start()
		{
			Task.Run(Run, _cancelTokenSource.Token);
		}

		public void Stop()
		{
			_isRunning = false;
		}

		private /*async Task*/void Run()
		{
			//logInfo("Starting UzScanner");

			Thread.Sleep(TimeSpan.FromSeconds(_initialDelay));
			//await Task.Delay(TimeSpan.FromSeconds(_initialDelay));

			_isRunning = true;
			//TODO: stats?

			while (_isRunning)
			{
				foreach (var statePair in _scanStates)
				{
					//await ScanAsync(statePair.Key, statePair.Value);
					ScanAsync(statePair.Key, statePair.Value).GetAwaiter().GetResult();
				}

				Thread.Sleep(TimeSpan.FromSeconds(_delay));
				//await Task.Delay(TimeSpan.FromSeconds(_delay));
			}
		}

		private UzService CreateService()
		{
			return new UzService(_settings.BaseUrl, _settings.SessionCookie, _proxyProvider, _log);
		}

		private void HandleError(string scanId, ScanData data, string error, bool fatal)
		{
			// TODO: Logging
			data.SetError(error);
		}

		private async Task ScanAsync(string scanId, ScanData data)
		{
			using (var lck = data.GetLock())
			{
				if (lck.IsCaptured)
				{
					data.IncAttempts();

					using (var service = CreateService())
					{
						var item = data.Item;
						var train = await service.FetchTrainAsync(item.Date, item.Source, item.Destination, item.TrainNumber);

						if (train != null)
						{
							CoachType[] coachTypes;

							if (!String.IsNullOrEmpty(item.CoachType))
							{
								var coachType = FindCoachType(train, item.CoachType);

								if (coachType == null)
								{
									HandleError(scanId, data, $"Coach type {item.CoachType} not found", true);
									return;
								}

								coachTypes = new[] { coachType };
							}
							else
							{
								coachTypes = train.CoachTypes;
							}

							var sessionId = await BookAsync(train, coachTypes, item.FirstName, item.LastName);

							if (!String.IsNullOrEmpty(sessionId))
							{
								ScanEvent?.Invoke(this, new ScanEventArgs(item.CallbackId, sessionId));
								Abort(scanId);
							}
							else
							{
								HandleError(scanId, data, "No vacant seats", false);
							}
						}
						else
						{
							HandleError(scanId, data, $"Train {item.TrainNumber} not found", true);
						}
					}
				}
			}
		}

		private async Task<string> BookAsync(Train train, CoachType[] coachTypes, string firstName, string lastName)
		{
			using (var svc = CreateService())
			{
				foreach (var coachType in coachTypes)
				{
					var coaches = await svc.ListCoachesAsync(train, coachType);

					// TODO: Smart coach and seat selection algorithm
					foreach (var coach in coaches.OrderByDescending(c => c.PlacesCount))
					{
						var allSeats = await svc.ListSeatsAsync(train, coach);
						var seats = coach.GetSeats(allSeats);

						foreach (var seat in seats.OrderBy(s => (s.Number - 1) % 2).ThenBy(s => s.Price).ThenBy(s => s.Number))
						{
							try
							{
								await svc.BookSeatAsync(train, coach, seat, new Passenger { FirstName = firstName, LastName = lastName, Bedding = coach.HasBedding });
							}
							catch (ResponseException)
							{
								continue;
							}

							return svc.GetSessionId();
						}
					}
				}
			}

			return null;
		}

		private static CoachType FindCoachType(Train train, string coachType)
		{
			return train.CoachTypes.FirstOrDefault(ct => ct.Letter == coachType);
		}
	}
}
