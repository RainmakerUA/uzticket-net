using System.Threading.Tasks;

namespace RM.UzTicket.Telegram.Contracts
{
	public interface ITelegramBot
	{
		Task SendMasterMessage(string message);
	}
}
