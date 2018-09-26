using RM.UzTicket.Contracts.DataContracts;

namespace RM.UzTicket.Contracts.ServiceContracts
{
	public interface ISettingsProvider
	{
		ISettings GetSettings();
	}
}
