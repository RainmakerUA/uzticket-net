using RM.UzTicket.Settings.Contracts;

namespace RM.UzTicket.Settings
{
	internal class SettingsData : ISettings
	{
		public SettingsData(string proxySource, string proxyScriptPath, string proxyPath, string proxyRegex, string teleBotKey)
		{
			ProxySource = proxySource;
			ProxyScriptPath = proxyScriptPath;
			ProxyPath = proxyPath;
			ProxyRegex = proxyRegex;
			TeleBotKey = teleBotKey;
		}

		public string ProxySource { get; }

		public string ProxyScriptPath { get; }

		public string ProxyPath { get; }

		public string ProxyRegex { get; }

		public string TeleBotKey { get; }
	}
}
