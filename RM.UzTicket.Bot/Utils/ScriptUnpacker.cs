using System;
using System.Text.RegularExpressions;

namespace RM.UzTicket.Bot.Utils
{
	internal class ScriptUnpacker
	{
		private class Unbaser
		{
			private const string _alphabet62 = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
			//private const string _alphabet95 = "\u0020!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

			private readonly int _base;

			public Unbaser(int @base)
			{
				if (@base > 62)
				{
					throw new ArgumentException($"Dunno what to do with base {_base}!");
				}

				_base = @base;
			}

			public int ToInt32(string str)
			{
				var result = 0;

				for (int i = str.Length - 1; i >= 0; i--)
				{
					result += (int)(Math.Pow(_base, i) * CharToInt(str[i]));
				}

				return result;
			}

			private int CharToInt(char @char)
			{
				return _alphabet62.IndexOf(@char, StringComparison.InvariantCulture);
			}
		}

		private struct Args
		{
			public readonly string Payload;
			public readonly string[] Symtab;
			public readonly int Radix;
			public readonly int Count;

			public Args(string payload, string[] symtab, int radix, int count)
			{
				Payload = payload;
				Symtab = symtab;
				Radix = radix;
				Count = count;
			}

			public string GetSymtabEntry(int index, string fallback)
			{
				var sym = Symtab[index];
				return !String.IsNullOrEmpty(sym) ? sym : fallback;
			}
		}

		private static readonly Regex _packArgRegex = new Regex(
												@"^eval\(function\((?'p'[a-z])(?:,[a-z]){5}\)\{.*return \k'p'\}\('(?'arg1'.+)',(?'arg2'\d+|\[\]),(?'arg3'\d+),'(?'arg4'[^']+)'\.split\('(?'split'[^']+)'\)(?:,0,\{\}\)\)|.*)$",
												RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
											);
		private static readonly Regex _payloadRegex = new Regex(@"\b\w+\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		public static string Unpack(string packed)
		{
			var args = FilterArgs(packed);

			if (args.Count != args.Symtab.Length)
			{
				throw new NotSupportedException("Malformed p.a.c.k.e.r symtab!");
			}

			var unbaser = new Unbaser(args.Radix);
			var replaced = _payloadRegex.Replace(args.Payload, m => { var word = m.Groups[0].Value; return args.GetSymtabEntry(unbaser.ToInt32(word), word); });

			return replaced;
		}

		private static Args FilterArgs(string source)
		{
			var match = _packArgRegex.Match(source);

			if (!match.Success)
			{
				throw new NotSupportedException("p.a.c.k.e.r is not found or wrong/unsupported data structure!");
			}

			var groups = match.Groups;
			var split = Regex.Unescape(groups["split"].Value);
			
			return new Args(
						Regex.Unescape(groups["arg1"].Value),
						Regex.Unescape(groups["arg4"].Value).Split(split[0]),
						Int32.TryParse(groups["arg2"].Value, out var radix) ? radix : 62,
						Int32.Parse(groups["arg3"].Value)
					);
		}
	}
}
