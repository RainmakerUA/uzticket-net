using System.Collections.Generic;

namespace RM.Lib.UzTicket.Model
{
	public class Seat : IPersistable
	{
		public string CharLine { get; set; }

		public int Number { get; set; }

		public decimal? Price { get; set; }

		public IDictionary<string, string> ToDictionary(IDictionary<string, string> src = null)
		{
			if (src == null)
			{
				src = new Dictionary<string, string>();
			}

			src["charline"] = CharLine;
			src["place_num"] = Number.ToString();

			return src;
		}

		public static Seat Create(string charline, int number, decimal? price = null)
		{
			return new Seat { CharLine = charline, Number = number, Price = price };
		}
	}
}
