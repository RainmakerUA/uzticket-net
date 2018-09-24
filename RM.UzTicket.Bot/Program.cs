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
		private static readonly AutoResetEvent _locker = new AutoResetEvent(false);
		private static ProxyManager _proxyMgr;

		private static void Main(string[] args)
		{
			Console.InputEncoding = Encoding.UTF8;
			Console.OutputEncoding = Encoding.UTF8;
			Console.CancelKeyPress += (sender, eventArgs) => { _locker.Set();  };

			var settings = Settings.Current;

			_proxyMgr = new ProxyManager(settings);

			var bot = new TelegramBotClient(settings.TeleBotKey);
			bot.OnMessage += BotOnOnMessage;

			bot.StartReceiving();

			RunBot(bot);

			var asyncLock = new Utils.AsyncLock(_locker);
			
			//Console.ReadLine();

			bot.StopReceiving();

			asyncLock.Dispose();
		}

		private static async void RunBot(TelegramBotClient bot)
		{
			//var asyncLock = new Utils.AsyncLock(_locker);

			try
			{
				var me = await bot.GetMeAsync();
				Console.WriteLine("Bot online: {0}{1}Press [Ctrl+C] to stop bot", me.Username, Environment.NewLine);
				//Console.ReadLine();
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
