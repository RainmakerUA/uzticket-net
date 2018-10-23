using System;
using System.Threading.Tasks;
using RM.Lib.Common.Contracts.Log;
using RM.Lib.Hosting.Contracts;
using RM.Lib.Proxy.Contracts;
using RM.Lib.Utility;
using RM.Lib.UzTicket.Contracts;
using RM.Lib.UzTicket.Model;
using RM.UzTicket.Settings.Contracts;
using RM.UzTicket.Telegram.Contracts;

namespace RM.Lib.UzTicket
{
	internal class UzClient : IUzClient
	{
		private readonly ISettingsProvider _settingsProvider;
		private readonly IProxyProvider _proxyProvider;

		private readonly IUzSettings _settings;
		private readonly UzScanner _scanner;

		public UzClient(ISettingsProvider settingsProvider, IProxyProvider proxyProvider)
		{
			_settingsProvider = settingsProvider;
			_proxyProvider = proxyProvider;

			_settings = _settingsProvider.GetSettings().UzService;
			_scanner = new UzScanner(_settings, _proxyProvider, Logger);
		}

		private static ILog Logger => LogFactory.GetLog();

		public async Task<string[]> GetStationsAsync(string name)
		{
			using (var svc = CreateService())
			{
				return Array.ConvertAll(await svc.SearchStationAsync(name), st => st.ToString());
			}
		}

		/*
	    public Task<Station> GetFirstStationAsync(string name)
	    {
		    return _service.FetchFirstStationAsync(name);
	    }

	    public Task<Train[]> GetTrainsAsync(DateTime date, Station source, Station destination)
	    {
		    return _service.ListTrainsAsync(date, source, destination);
	    }
		*/

		public static void Initialize(IDependencyResolver resolver)
		{
			var uzClient = (UzClient)resolver.Get<IUzClient>();
			var teleClient = resolver.Get<ITelegramBot>();
			var host = resolver.Get<IApplicationHost>();

			var scanner = uzClient._scanner;

			scanner.ScanEvent += async (o, e) => await teleClient.SendMasterMessage($"<b>Scan successful!</b>\nTicket reserved for '{e.CallbackID}':\n{e.Message}");
			host.Started += (o, e) => scanner.Start();
			host.Stopping += (o, e) => scanner.Stop();

			uzClient.LoadScans();//.GetAwaiter().GetResult();
		}

		private UzService CreateService()
		{
			return new UzService(_settings.BaseUrl, _settings.SessionCookie, _proxyProvider, Logger);
		}

		private async /*Task*/void LoadScans()
		{
			var temp = _settings.Temp;
			var scanParts = temp.Split('|');

			using (var service = CreateService())
			{
				var stFrom = await service.FetchFirstStationAsync(scanParts[0]);
				var stTo = await service.FetchFirstStationAsync(scanParts[1]);
				var date = DateTime.ParseExact(scanParts[2], "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
				var train = scanParts[3];
				var coach = scanParts[4];
				var firstName = scanParts[5];
				var lastName = scanParts[6];

				_scanner.AddItem(new ScanItem(
										temp, firstName, lastName,
										date, stFrom, stTo,
										train, coach
									));
			}
		}
	}
}
