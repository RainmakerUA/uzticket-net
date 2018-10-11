using System;
using RM.Lib.Common.Contracts.Log;
using RM.Lib.Hosting.Contracts;
using RM.Lib.Proxy.Contracts;
using RM.Lib.Utility;
using RM.UzTicket.Settings.Contracts;
using RM.UzTicket.Telegram.Contracts;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace RM.UzTicket.Telegram
{
	internal class TelegramBot : ITelegramBot
	{
		private readonly ISettingsProvider _settingsProvider;
		private readonly IProxyProvider _proxyProvider;
		private readonly ILog _log;
		private readonly ITelegramBotClient _client;

		public TelegramBot(ISettingsProvider settingsProvider, IProxyProvider proxyProvider)
		{
			_settingsProvider = settingsProvider;
			_proxyProvider = proxyProvider;
			_log = LogFactory.GetLog();

			var settings = _settingsProvider.GetSettings();

			_client = new TelegramBotClient(settings.TeleBotKey);
			_client.OnMessage += BotOnMessage;
		}
		
		public static void Initialize(IDependencyResolver resolver)
		{
			var host = resolver.Get<IApplicationHost>();
			var bot = (TelegramBot)resolver.Get<ITelegramBot>();

			host.Started += bot.OnHostStarted;
			host.Stopping += bot.OnHostStopping;
		}

		private void OnHostStarted(object sender, EventArgs e)
		{
			_client.StartReceiving();
		}

		private void OnHostStopping(object sender, EventArgs e)
		{
			_client.StopReceiving();
		}

		private async void BotOnMessage(object sender, MessageEventArgs e)
		{
			var proxy = await _proxyProvider.GetProxyAsync();

			_log.Info("Got message: {0} from {1} with locale {2}", e.Message.Text, e.Message.From.Username, e.Message.From.LanguageCode);
			_log.Info("Got proxy: {0}", proxy);

			if (sender is ITelegramBotClient iBot)
			{
				var msg = await iBot.SendTextMessageAsync(e.Message.Chat.Id, $"Got your <em>message</em>. Use proxy: {proxy}", ParseMode.Html, replyToMessageId: e.Message.MessageId);
			}
		}
	}
}
