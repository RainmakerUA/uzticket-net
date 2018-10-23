using System;
using System.Reflection;
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
		
		
		public string BaseUrl { get; private set; }
		
		public string SessionCookie { get; private set; }

		public int? ScanDelay { get; private set; }

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
			BotToken = Deobfuscate(token, 40);
			MasterChatID = Int64.TryParse(master, out var id) ? id : new long?();

			return this;
		}

		public SettingsData SetPersistenceSettings(string dbUrl, string dbPass)
		{
			DatabaseUrl = dbUrl;
			DatabasePassword = Deobfuscate(dbPass, 50);

			return this;
		}

		public SettingsData SetUzSettings(string baseUrl, string cookie, string scanDelay, string temp)
		{
			BaseUrl = baseUrl;
			SessionCookie = cookie;
			ScanDelay = Int32.TryParse(scanDelay, out var delay) ? delay : new int?();
			Temp = temp;

			return this;
		}

		private static string Deobfuscate(string str, byte code)
		{
#if DEBUG
			var resultLength = str.Length / 2;
			var result = new char[resultLength];

			for (int i = 0; i < resultLength; i++)
			{
				char ch1 = str[2 * i], ch2 = str[2 * i + 1];
				result[i] = (char)((ch1 - 0x30) * 10 + (ch2 - 0x30) + code);
			}

			return new string(result);
#else
			return str;
#endif
		}
	}
}
