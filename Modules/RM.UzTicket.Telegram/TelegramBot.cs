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
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BotMessageEventArgs = Telegram.Bot.Args.MessageEventArgs;
using MessageEventArgs = RM.UzTicket.Telegram.Contracts.MessageEventArgs;

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
			_log = LogFactory.GetLog(GetType());

			_settings = _settingsProvider.GetSettings().Telegram;

			_masterChatID = _settings.MasterChatID;

			_client = new TelegramBotClient(_settings.BotToken);
			_client.OnMessage += BotOnMessage;
			_client.OnCallbackQuery += BotCallbackQuery;
			_client.OnReceiveError += BotReceiveError;
			_client.OnReceiveGeneralError += BotReceiveGeneralError;
		}

		public event EventHandler<CommandEventArgs> Command;

		public event EventHandler<MessageEventArgs> Message;

		public event EventHandler<ResponseEventArgs> Response;

		public event EventHandler<ErrorEventArgs> Error;

		public Task SendMessageAsync(long id, string message)
		{
			return id == 0
						? SendMasterMessageAsync(message)
						: _client.SendTextMessageAsync(id, message, ParseMode.Html);
		}

		public Task SendMasterMessageAsync(string message)
		{
			return _masterChatID.HasValue ? SendMessageAsync(_masterChatID.Value, message) : Task.CompletedTask;
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

		private static readonly IReplyMarkup _testMarkup = new InlineKeyboardMarkup(new[] {
				                                                                            new[] { new InlineKeyboardButton { Text = "\U0001F1FA\U0001F1E6 UA", CallbackData = "UA" } },
				                                                                            new[] { new InlineKeyboardButton { Text = "\U0001F1F7\U0001F1FA RU", CallbackData = "RU" } },
				                                                                            new[] { new InlineKeyboardButton { Text = "\U0001F1FA\U0001F1F8 EN", CallbackData = "EN" } }

		                                                                            });
		private static readonly InlineKeyboardMarkup _testMarkup2 = new InlineKeyboardMarkup(new[] {
																								new[] { new InlineKeyboardButton { Text = "\u2705 Scan success!", CallbackData = "S-OK" } },
				                                                                                new[] { new InlineKeyboardButton { Text = "\u2757 Scan warning.", CallbackData = "S-WARN" } },
				                                                                                new[] { new InlineKeyboardButton { Text = "\u274C Scan error!", CallbackData = "S-ERR" } }

		                                                                                  });
		/*new ReplyKeyboardMarkup(new []
																					{
																						new[] { new KeyboardButton { Text = "\U0001F1FA\U0001F1E6 UA" } },
																						new[] { new KeyboardButton { Text = "\U0001F1F7\U0001F1FA RU" } },
																						new[] { new KeyboardButton { Text = "\U0001F1FA\U0001F1F8 EN" } }
		                                                                            });*/

		private void BotOnMessage(object sender, BotMessageEventArgs e)
		{
			var message = e.Message;
			var messageFrom = message.From;

#if DEBUG
			if (sender is ITelegramBotClient iBot && message.From.Id == _masterChatID)
			{
				var sendTask = e.Message.Text.Equals("/test", StringComparison.InvariantCultureIgnoreCase)
								? iBot.SendTextMessageAsync(message.Chat, "Please choose your language", replyMarkup: _testMarkup)
								: iBot.SendTextMessageAsync(message.Chat, $"Got your <em>message</em>", ParseMode.Html, replyMarkup: new ReplyKeyboardRemove(), replyToMessageId: message.MessageId);
				var msg = sendTask.GetAwaiter().GetResult();
			}
#endif

			if (message.Type == MessageType.Text)
			{
				var entities = message.Entities;
				var text = message.Text;

				if (entities != null && entities.Length > 0 && entities[0] is MessageEntity entity
						&& entity.Type == MessageEntityType.BotCommand && entity.Offset == 0)
				{
					if (ParseCommand(text, entity) is Command command)
					{
						var args = text.Substring(entity.Length).Split("\u0020".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						_log.Debug("TeleBot got command {0}({1})", command, String.Join(", ", args));
						Command?.Invoke(this, new CommandEventArgs(messageFrom.Id, command, args));
					}
					else
					{
						_log.Info("TeleBot got unrecognized command message: " + text);
						// TODO: Unknown command?
					}
				}
				else
				{
					_log.Debug("TeleBot got message: " + text);
					Message?.Invoke(this, new MessageEventArgs(messageFrom.Id, text));
				}
			}
		}

		private void BotCallbackQuery(object sender, CallbackQueryEventArgs e)
		{
			var query = e.CallbackQuery;
			var from = query.From.Id;
			var message = query.Message;
			var messageID = message?.MessageId;
			var data = query.Data;

			_log.Debug("Query Callback from {0}: \"{1}\" for message ID = {2}", query.From.Username, query.Data, messageID?.ToString() ?? "(no mID)");

#if DEBUG
			if (sender is ITelegramBotClient iBot && from == _masterChatID && message != null)
			{
				var rMarkup = data.StartsWith("S-") ? null : _testMarkup2;
				var sendTask = iBot.EditMessageTextAsync(new ChatId(from), messageID.Value, $"{message.Text}\nAnswer: <i>{data}</i>", ParseMode.Html, replyMarkup: rMarkup);
				var msg = sendTask.GetAwaiter().GetResult();
			}
#endif

			Response?.Invoke(this, new ResponseEventArgs(from, messageID ?? 0, data));
		}

		private void BotReceiveError(object sender, ReceiveErrorEventArgs e)
		{
			HandleError(e.ApiRequestException);
		}

		private void BotReceiveGeneralError(object sender, ReceiveGeneralErrorEventArgs e)
		{
			HandleError(e.Exception);
		}

		private void HandleError(Exception exc)
		{
			var errorHandler = Error;

			_log.Error("TeleBot Error: " + Environment.NewLine + exc);

			if (errorHandler != null)
			{
				errorHandler.Invoke(this, new ErrorEventArgs(exc));
			}
#if !DEBUG
			else
			{
				throw new Exception("Telegram exception", exc);
			}
#endif
		}

		private static Command? ParseCommand(string text, MessageEntity commandEntity)
		{
			var commandName = text.Substring(commandEntity.Offset + 1, commandEntity.Length - 1);
			return Enum.TryParse(commandName, true, out Command command) ? command : new Command?();
		}
	}
}
