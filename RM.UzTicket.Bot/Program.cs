using System;
using System.Text;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace RM.UzTicket.Bot
{
	internal static class Program
	{
		private class Closure
		{
			private readonly AutoResetEvent _event;

			public Closure(AutoResetEvent @event)
			{
				_event = @event;
			}

			public void CancelKeyPressHandler(object sender, ConsoleCancelEventArgs e)
			{
				if (!_event.SafeWaitHandle.IsClosed)
				{
					_event.Set();
				}
			}
		}

		private static ProxyManager _proxyMgr;

		private static void Main(string[] args)
		{
			using (var locker = new AutoResetEvent(false))
			{
				var closure = new Closure(locker);

				Console.InputEncoding = Encoding.UTF8;
				Console.OutputEncoding = Encoding.UTF8;
				Console.CancelKeyPress += closure.CancelKeyPressHandler;

				var settings = Settings.Current;

				_proxyMgr = new ProxyManager(settings);

				var bot = new TelegramBotClient(settings.TeleBotKey);
				bot.OnMessage += BotOnOnMessage;

				bot.StartReceiving();

				RunBot(bot, locker);

				locker.WaitOne();

				bot.StopReceiving();

				Console.CancelKeyPress -= closure.CancelKeyPressHandler;

				//Console.ReadLine();
			}
		}

		private static async void RunBot(TelegramBotClient bot, AutoResetEvent ev)
		{
			//var asyncLock = new Utils.AsyncLock(_locker);

			try
			{
				var me = await bot.GetMeAsync();
				Console.WriteLine("Bot online: {0}{1}Press [Ctrl+C] to stop bot", me.Username, Environment.NewLine);
				Console.ReadLine();
				Console.WriteLine("Got proxy: {0}", await _proxyMgr.GetProxy());
				//ev.Set();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				//Console.ReadLine();
			}
			finally
			{
				//asyncLock.Dispose();
			}
			
		}

		private static async void BotOnOnMessage(object sender, MessageEventArgs e)
		{
			var proxy = await _proxyMgr.GetProxy();

			Console.WriteLine("Got message: {0} from {1} with locale {2}", e.Message.Text, e.Message.From.Username, e.Message.From.LanguageCode);
			Console.WriteLine($"Got proxy: {proxy}");

			if (sender is ITelegramBotClient iBot)
			{
				var msg = await iBot.SendTextMessageAsync(e.Message.Chat.Id, $"Got your <em>message</em>. Use proxy: {proxy}", ParseMode.Html, replyToMessageId: e.Message.MessageId);
			}
		}
	}
}
