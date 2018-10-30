
namespace RM.UzTicket.Settings.Contracts
{
	public interface ISettings
	{
		IProxySettings Proxy { get; }
		
		ITelegramSettings Telegram { get; }
		
		IPersistenceSettings Persistence { get; }
		
		IUzSettings UzService { get; }
	}
}
