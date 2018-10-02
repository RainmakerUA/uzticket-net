using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RM.UzTicket.Proxy.Utils
{
	internal class Calculator
	{
		private struct CalcData
		{
			public readonly int Arg1;
			public readonly int Arg2;
			public readonly string Op;

			public CalcData(int arg1, int arg2, string op)
			{
				Arg1 = arg1;
				Arg2 = arg2;
				Op = op;
			}
		}

		private static readonly Regex _assignRegex = new Regex(@"^\s*(?'result'\w+)\s*=\s*(?'val'\w+)\s*$", RegexOptions.Compiled);
		private static readonly Regex _assignOpRegex = new Regex(@"^\s*(?'result'\w+)\s*=\s*(?'arg1'\w+)\s*(?'op'[^+])\s*(?'arg2'\w+)\s*$", RegexOptions.Compiled);
		private static readonly Regex _simpleRegex = new Regex(@"^\s*\(?\s*(?'arg1'\w+)\s*(?'op'[^+])\s*(?'arg2'\w+)\s*\)?\s*$");

		private readonly IDictionary<string, int> _varDict = new Dictionary<string, int>();

		public Calculator(string initData)
		{
			ParseInitData(initData);
		}

		public int CalcValue(string expression)
		{
			Match match;

			if ((match = _simpleRegex.Match(expression)).Success)
			{
				var calcData = ParseExpression(match.Groups["arg1"].Value, match.Groups["arg2"].Value, match.Groups["op"].Value);
				return Calc(calcData);
			}

			throw new ArgumentException($"unsupported expression '{expression}'!");
		}

		private void ParseInitData(string initData)
		{
			foreach (var expr in initData.Split(";", StringSplitOptions.RemoveEmptyEntries))
			{
				Match match;

				if ((match = _assignRegex.Match(expr)).Success)
				{
					_varDict[match.Groups["result"].Value] = ParseValue(match.Groups["val"].Value);
				}
				else if ((match = _assignOpRegex.Match(expr)).Success)
				{
					var calcData = ParseExpression(match.Groups["arg1"].Value, match.Groups["arg2"].Value, match.Groups["op"].Value);
					_varDict[match.Groups["result"].Value] = Calc(calcData);
				}

				// Ignore wrong lines
			}
		}

		private int ParseValue(string value)
		{
			return Int32.TryParse(value, out var num) || _varDict.TryGetValue(value, out num)
						? num
						: throw new ArgumentException($"Cannot find variable '{value}'!");
		}

		private CalcData ParseExpression(string arg1, string arg2, string op)
		{
			return new CalcData(ParseValue(arg1), ParseValue(arg2), op);
		}

		private static int Calc(CalcData data)
		{
			switch (data.Op)
			{
				case "+":
					return data.Arg1 + data.Arg2;

				case "^":
					return data.Arg1 ^ data.Arg2;

				default:
					throw new NotSupportedException($"Operation '{data.Op}' is not supported!");
			}
		}
	}
}
