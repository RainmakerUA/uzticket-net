using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RM.Lib.Hosting.Contracts;
using RM.Lib.Utility;
using RM.Lib.UzTicket.Contracts;
using RM.Lib.UzTicket.Contracts.DataContracts;
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

		//public static async Task Test()
		//{
		//	var settingsProvider = SettingsProvider.Load();
		//	var dataProvider = new PersistenceProvider(settingsProvider);
		//	var nums = new[] { 2134L, 4141L, 090241L, 123456L, 832571L };

		//	using (var client = await dataProvider.GetClientAsync("proxy"))
		//	{
		//		var res = await client.SetListAsync("list", nums);
		//	}

		//	await Task.Delay(16 * 1000);

		//	using (var client = await dataProvider.GetClientAsync("proxy"))
		//	{
		//		var items = await client.GetListAsync<int>("list");

		//		Console.WriteLine("Proxy items:");

		//		foreach (var item in items)
		//		{
		//			Console.WriteLine(item);
		//		}
		//	}
		//}

		public static void Initialize(IDependencyResolver resolver)
		{
			var uzClient = resolver.Get<IUzClient>();
			var teleClient = resolver.Get<ITelegramBot>();

			uzClient.ScanEvent += (o, e) =>
			{
				if (e is UpgradeScanEventArgs upgradeArgs && upgradeArgs.Kind == UpgradeKind.Captcha)
				{
					try
					{
						upgradeArgs.ResponseTask = GetCaptchaResponse(teleClient, upgradeArgs.CallbackID ?? 0, upgradeArgs.DataType, upgradeArgs.Data, upgradeArgs.Message);
					}
					catch (TaskCanceledException)
					{
						// do not fill upgradeArgs.Response
					}
				}
				else
				{
					var header = _headers.TryGetValue(e.Type, out var scanHeader) ? scanHeader : null;
					teleClient.SendMessageAsync(e.CallbackID ?? 0, header == null ? e.Message : header + "\n" + e.Message);
				}
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

		private static Task<string> GetCaptchaResponse(ITelegramBot bot, long id, string mimeType, byte[] data, string title)
		{
			EventHandler<MessageEventArgs> onMessage = null;
			var tcs = new TaskCompletionSource<string>();
			var cs = new CancellationTokenSource(TimeSpan.FromMinutes(5));
			var token = cs.Token;
			var reg = token.Register(
						() => 
							{
								tcs.TrySetCanceled(token);
								bot.Message -= onMessage;
							},
						false
					);

			onMessage = (sender, e) =>
			{
				var handler = onMessage;
				bot.Message -= handler;
				reg.Dispose();

				tcs.SetResult(e.Message);
			};

			bot.Message += onMessage;
			
			return bot.SendImageAsync(id, mimeType, data, title).Then(msgID => tcs.Task);
		}
	}
}
