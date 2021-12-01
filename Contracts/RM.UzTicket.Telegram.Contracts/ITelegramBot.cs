using System;
using System.Threading.Tasks;

namespace RM.UzTicket.Telegram.Contracts
{
	public interface ITelegramBot
	{
		event EventHandler<CommandEventArgs> Command;

		event EventHandler<MessageEventArgs> Message;

		event EventHandler<ResponseEventArgs> Response;

		event EventHandler<ErrorEventArgs> Error;

		Task<int> SendMessageAsync(long id, string message);

		Task<int> SendImageAsync(long id, string mimeType, byte[] data, string title);

		Task<int> SendMasterMessageAsync(string message);

		Task SendTypingAsync(long id);
	}
}
