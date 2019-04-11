using System;
using RM.UzTicket.Settings.Contracts;

namespace RM.UzTicket.Settings
{
	internal class SettingsData : ISettings,
									IProxySettings, ITelegramSettings,
									IPersistenceSettings, IUzSettings 
	{
		public IProxySettings Proxy => this;

		public ITelegramSettings Telegram => this;

		public IPersistenceSettings Persistence => this;

		public IUzSettings UzService => this;


		public string SourceUrl { get; private set; }
		
		public string ScriptPath { get; private set; }
		
		public string ProxyPath { get; private set; }
		
		public string ProxyRegex { get; private set; }
		
		
		public string BotToken { get; private set; }
		
		public long? MasterChatID { get; private set; }
		
		
		public string DatabaseUrl { get; private set; }
		
		public string DatabasePassword { get; private set; }

		public string ConnectionString { get; private set; }
		
		
		public string BaseUrl { get; private set; }
		
		public string SessionCookie { get; private set; }

		public double? ScanDelay { get; private set; }

		public string Temp { get; private set; }

		public SettingsData SetProxySettings(string source, string scriptPath, string proxyPath, string proxyRegex)
		{
			SourceUrl = source;
			ScriptPath = scriptPath;
			ProxyPath = proxyPath;
			ProxyRegex = proxyRegex;
			
			return this;
		}

		public SettingsData SetTelegramSettings(string token, string master)
		{
			BotToken = Deobfuscator.TryDeobfuscate(token);
			MasterChatID = Int64.TryParse(master, out var id) ? id : new long?();

			return this;
		}

		public SettingsData SetPersistenceSettings(string dbUrl, string dbPass, string connStr)
		{
			DatabaseUrl = dbUrl;
			DatabasePassword = Deobfuscator.TryDeobfuscate(dbPass);
			ConnectionString = connStr;

			return this;
		}

		public SettingsData SetUzSettings(string baseUrl, string cookie, string scanDelay, string temp)
		{
			BaseUrl = baseUrl;
			SessionCookie = cookie;
			ScanDelay = Double.TryParse(scanDelay, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var delay) ? delay : new double?();
			Temp = temp;

			return this;
		}
	}
}
