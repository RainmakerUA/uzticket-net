
namespace RM.UzTicket.Settings.Contracts
{
	public interface ISettings
	{
		string ProxySource { get; }

		string ProxyScriptPath { get; }

		string ProxyPath { get; }

		string ProxyRegex { get; }

		string TeleBotKey { get; }
	}
}
