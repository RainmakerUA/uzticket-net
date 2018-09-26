using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using RM.UzTicket.Contracts.DataContracts;
using RM.UzTicket.Contracts.ServiceContracts;

namespace RM.UzTicket.Bot
{
	internal class SettingsProvider : ISettingsProvider
	{
		private class SettingsData : ISettings
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

		private const string _varPrefix = "UZTB_";
#if DEBUG
		private const string _env = ".env";
		private const string _envPattern = @"^\s*{0}\s*=\s*(?'val'\S*)\s*$";
#endif
		private static readonly string[] _varNames;

		private static SettingsProvider _current;

		private readonly IDictionary<string, string> _variables = new Dictionary<string, string>();

		static SettingsProvider()
		{
			_varNames = new[]
						{
							"PROXYSRC",
							"PROXYSCRIPTPATH",
							"PROXYPATH",
							"PROXYRE",
							"TELEBOTKEY",
			            }.Select(s => $"{_varPrefix}{s}").ToArray();
		}

		public SettingsProvider()
		{
			foreach (var varName in _varNames)
			{
				_variables.Add(varName, Environment.GetEnvironmentVariable(varName));
			}
		}

#if DEBUG
		public SettingsProvider(string envContent)
		{
			foreach (var varName in _varNames)
			{
				_variables.Add(varName, GetMatch(envContent, String.Format(_envPattern, varName)));
			}
		}

		private static string GetMatch(string content, string pattern)
		{
			var re = new Regex(pattern, RegexOptions.Multiline);
			var match = re.Match(content);

			return match.Success ? match.Groups["val"].Value : null;
		}
#endif

		public static SettingsProvider Current => _current ?? (_current = Load());
		
		public ISettings GetSettings()
		{
			return new SettingsData(
								GetVariable("PROXYSRC"), GetVariable("PROXYSCRIPTPATH"),
								GetVariable("PROXYPATH"), GetVariable("PROXYRE"),
								GetVariable("TELEBOTKEY")
							);
		}

		private string GetVariable(string name)
		{
			return _variables.TryGetValue(_varPrefix + name, out var value) ? value : null;
		}

		private static SettingsProvider Load()
		{
#if DEBUG
			var envFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _env);

			if (File.Exists(envFile))
			{
				return new SettingsProvider(File.ReadAllText(envFile));
			}
#endif
			return new SettingsProvider();
		}
	}
}
