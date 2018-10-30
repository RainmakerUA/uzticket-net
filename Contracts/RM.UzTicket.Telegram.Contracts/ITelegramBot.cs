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

		Task SendMessageAsync(long id, string message);

		Task SendMasterMessageAsync(string message);
	}
}
