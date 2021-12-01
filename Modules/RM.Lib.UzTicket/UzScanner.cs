using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using RM.Lib.Common.Contracts.Log;
using RM.Lib.Utility;
using RM.Lib.UzTicket.Contracts.DataContracts;
using RM.Lib.UzTicket.Exceptions;
using RM.Lib.UzTicket.Model;
using RM.Lib.UzTicket.Utils;
using RM.UzTicket.Settings.Contracts;

namespace RM.Lib.UzTicket
{
	internal sealed class UzScanner : IDisposable
	{
		[DataContract]
		private class ScanData
		{
			private const int _lockTimeout = 100;
			private readonly object _lock;

			public ScanData()
			{
				_lock = new object();
			}

			public ScanData(ScanItem item) : this()
			{
				Item = item;
			}

			[DataMember]
			public ScanEventType State { get; set; }

			[DataMember]
			public string StateDescription { get; set; }

			[DataMember]
			public ScanItem Item { get; set; }

			[DataMember]
			public int Attempts { get; set; }

			[DataMember]
			public int Errors { get; set; }

			public AsyncLock GetLock()
			{
				return AsyncLock.Lock(_lock, _lockTimeout);
			}
		}

		private const int _errorsToFail = 5; // TODO: Settings
		private const int _initialDelay = 10;
		private const int _defaultDelay = 15 * 60 + 30;

		private readonly IUzSettings _settings;
		private readonly ILog _log;
		private readonly Func<string, UzService> _serviceFactory;
		private readonly TimeSpan _delay;
		private readonly IDictionary<string, ScanData> _scanStates;

		private CancellationTokenSource _cancelTokenSource;
		private CancellationToken _cancelToken;
		private bool _isDisposed;
		private string _sessionID;

		public UzScanner(IUzSettings settings, ILog log, Func<string, UzService> serviceFactory)
		{
			_settings = settings;
			_log = log;
			_serviceFactory = serviceFactory;

			_delay = _settings.ScanDelay.HasValue ? TimeSpan.FromMinutes(_settings.ScanDelay.Value) : TimeSpan.FromSeconds(_defaultDelay);
			_scanStates = new ConcurrentDictionary<string, ScanData>();
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

		public async Task LoadScans()
		{
			var temp = _settings.Temp;
			var scanLines = temp.Split('#');

			using (var service = CreateService(_sessionID))
			{
				foreach (var line in scanLines)
				{
					try
					{
						_log.Debug($"Adding scan item [{line}]");

						var scanParts = line.Split('|');

						var callback = Int32.TryParse(scanParts[0], out var cbID) ? cbID : new int?();
						var (stFrom, stTo) = await Task.WhenAll(
																service.FetchFirstStationAsync(scanParts[1]),
																service.FetchFirstStationAsync(scanParts[2])
															);
						var date = DateTime.ParseExact(scanParts[3], "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
						var train = scanParts[4];
						var coach = scanParts[5];
						var firstName = scanParts[6];
						var lastName = scanParts[7];

						AddItem(new ScanItem
						{
							ScanSource = line,
							CallbackID = callback,
							FirstName = firstName,
							LastName = lastName,
							Date = date,
							Source = stFrom,
							Destination = stTo,
							TrainNumber = train,
							CoachType = coach,
						});

						_log.Debug($"Added scan item [{line}]");
					}
					catch (Exception e)
					{
						_log.Error("Error adding scan item `{0}`", e, line);
					}
				}
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

		public string[] GetStatus(long? callbackID)
		{
			return _scanStates.Values.Where(sd => !callbackID.HasValue || callbackID.Value == (sd.Item.CallbackID ?? 0))
										.Select(FormatScanData).ToArray();

			string FormatScanData(ScanData data)
			{
				const string successEmo = "\u2705";
				const string warningEmo = "\u2757";
				const string errorEmo = "\u274C";
				const string lookEmo = "\U0001F50D";
				const string clipboard = "\U0001F4CB";
				const string greyExclamation = "\u2755";
				const string heavyExclamation = "\u2757";
				const string greyQuestion = "\u2754";
				const string redQuestion = "\u2753";

				string header;
				string footer;

				switch (data.State)
				{
					case ScanEventType.None:
						header = lookEmo + $"<b>Scan in progress:</b> {data.Attempts} attempts";
						footer = !String.IsNullOrEmpty(data.StateDescription) ? greyQuestion + data.StateDescription : redQuestion + "(no state information)";
						break;

					case ScanEventType.Success:
						header = successEmo + $"<b>Scan successful</b>: {data.Attempts} attempts";
						footer = "Session ID: " + data.StateDescription;
						break;

					case ScanEventType.Warning:
						header = warningEmo + $"<b>Scan failing:</b> {data.Errors}/{_errorsToFail} errors, {data.Attempts} attempts";
						footer = greyExclamation + data.StateDescription;
						break;

					case ScanEventType.Error:
						header = errorEmo + $"<b>Scan failed:</b> {data.Errors} errors, {data.Attempts} attempts";
						footer = heavyExclamation + data.StateDescription;
						break;

					default:
						throw new ArgumentOutOfRangeException("Unknown Scan State!");
				}

				return header + "\n" + clipboard + data.Item.ScanSource + "\n" + footer;
			}
		}

		public Tuple<int, string> GetStatus(string scanId)
		{
			if (_scanStates.TryGetValue(scanId, out var data))
			{
				return Tuple.Create(data.Attempts, data.StateDescription);
			}

			throw new ScanNotFoundException(scanId);
		}

		public void Reset()
		{
			Stop();
			_scanStates.Clear();
		}

		public void Start()
		{
			if (_scanStates.Count == 0)
			{
				_log.Warning("Scan items are absent. Nothing to start!");
				return;
			}

			if (_cancelTokenSource == null)
			{
				_log.Debug("Start scanning");

				_cancelTokenSource = new CancellationTokenSource();
				_cancelToken = _cancelTokenSource.Token;

				Task.Run(Run, _cancelToken);
			}
		}

		public void Stop()
		{
			if (_cancelTokenSource != null)
			{
				_cancelTokenSource.Cancel();
				_cancelTokenSource = null;

				_log.Debug("Stopped scanning");
			}
		}

		private void Run()
		{
			_log.Debug("Run()");

			var isRunning = _cancelToken.WaitedOrCancelled(TimeSpan.FromSeconds(_initialDelay));

			//TODO: stats?

			using (var service = CreateService(_sessionID))
			{
				while (isRunning)
				{
					foreach (var statePair in _scanStates)
					{
						try
						{
							UpgradeRequiredException upgradeException = null;
							int count = 0;

							do
							{
								ScanAsync(service, statePair.Key, statePair.Value)
										.Then(
												() =>
												{
													upgradeException = null;
												},
												exception =>
												{
													if (exception is UpgradeRequiredException urExc
															&& urExc.Kind == UpgradeKind.Captcha
															&& count < _errorsToFail)
													{
														count++;
														upgradeException = urExc;

														var (imgType, imgData) = service.GetCaptchaImage().GetAwaiter().GetResult();
														var args = new UpgradeScanEventArgs(
																statePair.Value.Item.CallbackID,
																UpgradeKind.Captcha,
																imgType,
																imgData
														);

														ScanEvent?.Invoke(this, args);

														try
														{
															service.SetCaptchaResult(args.ResponseTask.GetAwaiter().GetResult());
														}
														catch (TaskCanceledException)
														{
															// Do nothing if cancelled
														}
													}
													else
													{
														HandleError(statePair.Key, statePair.Value, exception?.Message, true);
														upgradeException = null;
													}
												})
										.Wait(_cancelToken);
							}
							while (upgradeException != null);

							_sessionID = service.GetSessionId();
						}
						catch (Exception e)
						{
							HandleError(statePair.Key, statePair.Value, e.Message, true);
						}
					}

					isRunning = _cancelToken.WaitedOrCancelled(_delay);
				}
			}

			_log.Debug("Run() end");
		}

		private UzService CreateService(string sessionID)
		{
			return _serviceFactory?.Invoke(sessionID) ?? new UzService(logger: _log);
		}

		private void HandleError(string scanId, ScanData data, string error, bool severe)
		{
			const string itemFailed = "Item is failed: scanning skipped";

			if (String.IsNullOrEmpty(error))
			{
				error = "Task was cancelled";
				severe = false;
			}

			if (severe)
			{
				data.Errors++;

				var msg = $"Errors: {data.Errors} in a row on scanning [{data.Item.ScanSource}]: {error}";

				_log.Error(msg);
				data.StateDescription = error;

				if (data.Errors >= _errorsToFail)
				{
					data.State = ScanEventType.Error;
					ScanEvent?.Invoke(this, new ScanEventArgs(data.Item.CallbackID, ScanEventType.Error, msg + "\n" + itemFailed));
					_log.Error(itemFailed);
				}
				else
				{
					data.State = ScanEventType.Warning;
				}
			}
			else
			{
				data.State = ScanEventType.None;
				data.StateDescription = error;
				data.Errors = 0;
				_log.Warning($"Warning for scan [{data.Item.ScanSource}]: {error}");
			}
		}

		//private async Task ScanAsync(string scanId, ScanData data)
		private async Task ScanAsync(UzService service, string scanId, ScanData data)
		{
			if (data.State == ScanEventType.Error)
			{
				return;
			}

			using (var lck = data.GetLock())
			{
				if (lck.IsCaptured)
				{
					data.Attempts++;

					var item = data.Item;
					var trains = await service.ListTrainsAsync(item.Date, item.Source, item.Destination);

					if (trains != null && trains.Length > 0)
					{
						var train = trains.GetByNumber(item.TrainNumber);

						if (train != null)
						{
							if (train.CoachTypes != null && train.CoachTypes.Length > 0)
							{
								CoachType[] coachTypes = null;

								if (!String.IsNullOrEmpty(item.CoachType))
								{
									coachTypes = FindCoachTypes(train, item.CoachType);

									if (coachTypes.Length == 0 && item.ExactCoachType)
									{
										HandleError(scanId, data, "No vacant coaches " + item.CoachType, false);
										return;
									}
								}

								if (coachTypes == null || coachTypes.Length == 0)
								{
									coachTypes = train.CoachTypes;
								}

								var (sessionId, coach, seat) = await BookAsync(service, train, coachTypes, item.FirstName, item.LastName);

								if (!String.IsNullOrEmpty(sessionId))
								{
									var msg = $"Reserved ticket for [{item.ScanSource}]:\nTrain: {train.Number} Coach: {coach.Number}-{coach.Type} Seat: {seat.Number} Price: {(seat.Price ?? 0) / 100m:##.00 UAH}\n{sessionId}";
									_log.Info(msg);
									data.State = ScanEventType.Success;
									data.StateDescription = sessionId;
									ScanEvent?.Invoke(this, new ScanEventArgs(item.CallbackID, ScanEventType.Success, msg));
									//Abort(scanId); rebook again in case it is bought by requester
									return;
								}
							}

							HandleError(scanId, data, "No vacant seats", false);
						}
						else
						{
							HandleError(scanId, data, $"Train {item.TrainNumber} not found", false);
						}
					}
					else
					{
						HandleError(scanId, data, "No tickets selling for date/train", false);
					}
				}
			}
		}

		private async Task<(string, Coach, Seat)> BookAsync(UzService svc, Train train, CoachType[] coachTypes, string firstName, string lastName)
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
							return (svc.GetSessionId(), coach, seat);
						}
						catch (ResponseException)
						{
							// Just try next seat
						}
					}
				}
			}

			return default;
		}

		private static CoachType[] FindCoachTypes(Train train, string coachType)
		{
			return Array.FindAll(train.CoachTypes, ct => coachType.IndexOf(ct.Letter, StringComparison.InvariantCultureIgnoreCase) >= 0);
		}
	}
}
