using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RM.UzTicket.Bot
{
	internal class Settings
	{
		private const string _varPrefix = "UZTB_";
#if DEBUG
		private const string _env = ".env";
		private const string _envPattern = @"^\s*{0}\s*=\s*(?'val'\S*)\s*$";
#endif
		private static readonly string[] _varNames;

		private static Settings _current;

		private readonly IDictionary<string, string> _variables = new Dictionary<string, string>();

		static Settings()
		{
			_varNames = new[]
						{
							"PROXYSRC",
							"PROXYPATH",
							"PROXYRE",
							"TELEBOTKEY",
			            }.Select(s => $"{_varPrefix}{s}").ToArray();
		}

		public Settings()
		{
			foreach (var varName in _varNames)
			{
				_variables.Add(varName, Environment.GetEnvironmentVariable(varName));
			}
		}

#if DEBUG
		public Settings(string envContent)
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

		public static Settings Current => _current ?? (_current = Load());

		public string ProxySource => GetVariable("PROXYSRC");

		public string ProxyPath => GetVariable("PROXYPATH");

		public string ProxyRegex => GetVariable("PROXYRE");

		public string TeleBotKey => GetVariable("TELEBOTKEY");

		private string GetVariable(string name)
		{
			return _variables.TryGetValue(_varPrefix + name, out var value) ? value : null;
		}

		private static Settings Load()
		{
#if DEBUG
			var envFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _env);

			if (File.Exists(envFile))
			{
				return new Settings(File.ReadAllText(envFile));
			}
#endif
			return new Settings();
		}
	}
}
