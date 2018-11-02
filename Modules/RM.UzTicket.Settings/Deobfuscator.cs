using System;
using System.Text.RegularExpressions;

namespace RM.UzTicket.Settings
{
	internal static class Deobfuscator
	{
		private static readonly Regex _obfuscatedRegex = new Regex(
																	@"^X\:(?'text'\d+)\:(?'code'\d{1,2})$",
																	RegexOptions.Compiled | RegexOptions.CultureInvariant
																);

		public static string TryDeobfuscate(string input)
		{
			var match = _obfuscatedRegex.Match(input);

			if (match.Success)
			{
				var str = match.Groups["text"].Value;
				var code = Byte.Parse(match.Groups["code"].Value);
				var resultLength = str.Length / 2;
				var result = new char[resultLength];

				for (int i = 0; i < resultLength; i++)
				{
					char ch1 = str[2 * i], ch2 = str[2 * i + 1];
					result[i] = (char)((ch1 - 0x30) * 10 + (ch2 - 0x30) + code);
				}

				return new string(result);
			}

			return input;
		}
	}
}
