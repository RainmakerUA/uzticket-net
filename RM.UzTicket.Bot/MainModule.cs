using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RM.Lib.Hosting.Contracts;
using RM.Lib.UzTicket.Contracts;
using RM.Lib.UzTicket.Contracts.DataContracts;
using RM.UzTicket.Data;
using RM.UzTicket.Settings;
using RM.UzTicket.Telegram.Contracts;

namespace RM.UzTicket.Bot
{
	internal static class MainModule
	{
		private const string _successEmo = "\u2705";
		private const string _warningEmo = "\u2757";
		private const string _errorEmo = "\u274C";

		private static readonly IDictionary<ScanEventType, string> _headers = new Dictionary<ScanEventType, string>
																							{
																								[ScanEventType.Success] = _successEmo + " Scan success!",
																								[ScanEventType.Warning] = _warningEmo + " Scan warning.",
																								[ScanEventType.Error] = _errorEmo + " Scan error!"
																							};

		public static async Task Test()
		{
			var settingsProvider = SettingsProvider.Load();
			var dataProvider = new RedisDataProvider(settingsProvider);
			await dataProvider.ConnectAsync();

			var items = await dataProvider.GetProxyItemsAsync();

			Console.WriteLine("Proxy items:");

			foreach (var item in items)
			{
				Console.WriteLine(item);
			}
		}

		public static void Initialize(IDependencyResolver resolver)
		{
			var uzClient = resolver.Get<IUzClient>();
			var teleClient = resolver.Get<ITelegramBot>();

			uzClient.ScanEvent += (o, e) =>
			{
				var header = _headers.TryGetValue(e.Type, out var scanHeader) ? scanHeader : null;
				teleClient.SendMessageAsync(e.CallbackID ?? 0, header == null ? e.Message : header + "\n" + e.Message);
			};

			teleClient.Command += async (o, e) =>
			{
				switch (e.Command)
				{
					case Command.Status:
						var callback = e.IsMaster ? new long?() : e.Sender;
						await teleClient.SendTypingAsync(callback ?? 0);
						await SendScanStatus(teleClient, callback ?? 0, uzClient.GetScanStatus(callback));
						break;
					case Command.Reset:
						await uzClient.ResetScan();
						break;
				}
			};
		}

		private static Task SendScanStatus(ITelegramBot telebot, long sendTo, string[] statuses)
		{
			return telebot.SendMessageAsync(sendTo, statuses != null && statuses.Length > 0 ? String.Join("\n\n", statuses) : "You do not have any scans!");
		}
	}
}
