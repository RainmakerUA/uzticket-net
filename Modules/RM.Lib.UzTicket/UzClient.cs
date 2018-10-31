using System;
using System.Threading.Tasks;
using RM.Lib.Common.Contracts.Log;
using RM.Lib.Hosting.Contracts;
using RM.Lib.Proxy.Contracts;
using RM.Lib.Utility;
using RM.Lib.UzTicket.Contracts;
using RM.Lib.UzTicket.Contracts.DataContracts;
using RM.UzTicket.Settings.Contracts;

namespace RM.Lib.UzTicket
{
	internal sealed class UzClient : IUzClient
	{
		private readonly ISettingsProvider _settingsProvider;
		private readonly IProxyProvider _proxyProvider;

		private readonly ILog _logger;
		private readonly IUzSettings _settings;
		private readonly UzScanner _scanner;

		public UzClient(ISettingsProvider settingsProvider, IProxyProvider proxyProvider)
		{
			_settingsProvider = settingsProvider;
			_proxyProvider = proxyProvider;

			_logger = LogFactory.GetLog(nameof(UzClient));
			_settings = _settingsProvider.GetSettings().UzService;
			_scanner = new UzScanner(_settings, LogFactory.GetLog(nameof(UzScanner)), CreateService);
		}

		//public void StartScan()
		//{
		//	_scanner.Start();
		//}

		//public void StopScan()
		//{
		//	_scanner.Stop();
		//}

		public event EventHandler<ScanEventArgs> ScanEvent
		{
			add => _scanner.ScanEvent += value;
			remove => _scanner.ScanEvent -= value;
		}

		public async Task<Station[]> GetStationsAsync(string name)
		{
			using (var svc = CreateService())
			{
				return await svc.SearchStationAsync(name);
			}
		}

	    public async Task<Station> GetFirstStationAsync(string name)
	    {
		    using (var svc = CreateService())
		    {
			    return await svc.FetchFirstStationAsync(name);
		    }
	    }

		public async Task ResetScan()
		{
			_logger.Info("Resetting scanning...");
			_scanner.Reset();
			await _scanner.LoadScans();
			await Task.Delay(TimeSpan.FromSeconds(1));
			_scanner.Start();
			_logger.Info("Scanning restarted...");
		}

		/*
	    public Task<Train[]> GetTrainsAsync(DateTime date, Station source, Station destination)
	    {
		    return _service.ListTrainsAsync(date, source, destination);
	    }
		*/

		public static void Initialize(IDependencyResolver resolver)
		{
			var host = resolver.Get<IApplicationHost>();
			var uzClient = (UzClient)resolver.Get<IUzClient>();

			host.Started += uzClient.OnHostStarted;
			host.Stopping += uzClient.OnHostStopping;

			uzClient._scanner.LoadScans().Wait();
		}

		private UzService CreateService()
		{
			return new UzService(_settings.BaseUrl, _settings.SessionCookie, _proxyProvider, LogFactory.GetLog(nameof(UzService)));
		}

		private void OnHostStarted(object sender, EventArgs e)
		{
			_scanner.Start();
		}

		private void OnHostStopping(object sender, EventArgs e)
		{
			_scanner.Stop();
		}
	}
}
