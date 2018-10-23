using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using RM.Lib.UzTicket.Utils;

namespace RM.Lib.UzTicket.Model
{
	[DataContract]
	public class Coach : IPersistable
	{
		[DataMember]
		public bool AllowBonus { get; set; }

		[DataMember]
		public string Class { get; set; }

		[DataMember]
		public string Type { get; set; }

		[DataMember]
		public int Railway { get; set; }

		[DataMember]
		public bool HasBedding { get; set; }

		[DataMember(Name = "num")]
		public int Number { get; set; }

		[DataMember(Name = "free")]
		public int PlacesCount { get; set; }

		[DataMember(Name = "prices")]
		public IDictionary<string, long> Prices { get; set; }

		[DataMember]
		public long ReservePrice { get; set; }

		[DataMember]
		public string[] Services { get; set; }

		[DataMember]
		public bool ByWishes { get; set; }

		[DataMember(Name = "air")]
		public bool? AirConditioning { get; set; }

		public string RailwayName => RailwayNames.GetName(Railway, false);

		public bool AirConditioningAvailable => AirConditioning ?? true;

		public string GetInfo()
		{
			var list = new List<string>
							{
								"Coach #{0} class {1} {2}",
								"Places: {3}",
								"Air Cond.: {4}",
								"Bedding: {5}",
								"Services: {6}",
								"~~~~~~~~~~~~~"
							};
			var services = String.Join("\u0020", Services);
			list.AddRange(Prices.Select(kv => $"{kv.Key}: {(decimal)kv.Value/100} UAH"));
			return String.Format(
							String.Join(Environment.NewLine, list),
							Number, Class, RailwayName,
							PlacesCount,
							AirConditioningAvailable ? "+" : "-",
							HasBedding ? "+" : "-",
							services
						);
		}

		public override string ToString()
		{
			return $"Coach {Number}";
		}

		public IDictionary<string, string> ToDictionary(IDictionary<string, string> src = null)
		{
			if (src == null)
			{
				src = new Dictionary<string, string>();
			}

			src["wagon_num"] = Number.ToString();
			src["wagon_type"] = Type;
			src["wagon_class"] = Class;
			//src["wagon_railway"] = Railway.ToString();
			
			return src;
		}
	}
}
