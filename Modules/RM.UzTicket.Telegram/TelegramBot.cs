using System;
using System.Threading.Tasks;
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
		private readonly ITelegramSettings _settings;
		private readonly IProxyProvider _proxyProvider;
		private readonly ILog _log;
		private readonly ITelegramBotClient _client;
		private readonly long? _masterChatID;

		public TelegramBot(ISettingsProvider settingsProvider, IProxyProvider proxyProvider)
		{
			_settingsProvider = settingsProvider;
			_proxyProvider = proxyProvider;
			_log = LogFactory.GetLog();

			_settings = _settingsProvider.GetSettings().Telegram;

			_masterChatID = _settings.MasterChatID;

			_client = new TelegramBotClient(_settings.BotToken);
			_client.OnMessage += BotOnMessage;
		}

		public async Task SendMasterMessage(string message)
		{
			if (_masterChatID.HasValue)
			{
				await _client.SendTextMessageAsync(_masterChatID, message, ParseMode.Html);
			}
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
			var message = e.Message;
			var messageFrom = message.From;
			
			_log.Info("Got message: {0} from {1} with locale {2}", message.Text, messageFrom.Username, messageFrom.LanguageCode);
			_log.Info("Got proxy: {0}", proxy);

			if (sender is ITelegramBotClient iBot)
			{
				var msg = await iBot.SendTextMessageAsync(message.Chat, $"Got your <em>message</em>. Use proxy: {proxy}", ParseMode.Html, replyToMessageId: message.MessageId);
			}

			if (message.Chat.Id != _masterChatID)
			{
				var user = $"{messageFrom.Username} ({messageFrom.Id}) - {messageFrom.FirstName} {messageFrom.LastName}";
				await SendMasterMessage($"Sent proxy to {user}");
			}
		}
	}
}
