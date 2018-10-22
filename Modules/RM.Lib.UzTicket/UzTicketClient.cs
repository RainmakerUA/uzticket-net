using System;
using System.Threading.Tasks;
using RM.Lib.UzTicket;
using RM.UzTicket.Lib.Model;

namespace RM.UzTicket.Lib
{
	public sealed class UzTicketClient : IDisposable
	{
		private readonly UzService _service;
		private readonly UzScanner _scanner;
		private bool _isDisposed;

		public UzTicketClient()
		{
			_service = new UzService();
			_scanner = new UzScanner(ScanCallbackAsync, 120);
		}

		#region Disposable

		~UzTicketClient()
		{
			Dispose(false);
		}

		private void Dispose(bool isDisposing)
		{
			if (!_isDisposed)
			{
				if (isDisposing)
				{
					// Free managed resources
					_service.Dispose();
					_scanner.Dispose();
				}

				// Free unmanagement resources
				// ...

				_isDisposed = true;
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		#endregion

		//public event EventHandler<ScanResult<string>>  ScanResult;

		#region Methods

		public Task<Station[]> GetStationsAsync(string name)
		{
			return _service.SearchStationAsync(name);
		}

		public Task<Station> GetFirstStationAsync(string name)
		{
			return _service.FetchFirstStationAsync(name);
		}

		public Task<Train[]> GetTrainsAsync(DateTime date, Station source, Station destination)
		{
			return _service.ListTrainsAsync(date, source, destination);
		}

		#endregion







		private Task ScanCallbackAsync(string scanId, string sessionId)
		{
			//ScanResult?.Invoke(this, new ScanResult<string>(scanId, $"document.cookie='{sessionId}'"));
			return Task.Delay(0);
		}
	}
}
