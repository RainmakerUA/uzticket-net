using System;
using System.Threading.Tasks;

namespace RM.UzTicket.Telegram.Contracts
{
	public interface ITelegramBot
	{
		event EventHandler<CommandEventArgs> Command;

		event EventHandler<MessageEventArgs> Message;

		event EventHandler<ErrorEventArgs> Error;

		Task SendMasterMessage(string message);
	}
}
