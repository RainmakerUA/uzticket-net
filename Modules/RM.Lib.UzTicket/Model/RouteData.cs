using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RM.Lib.UzTicket.Utils;

namespace RM.Lib.UzTicket.Model
{
	[DataContract]
	public class RouteData : IPersistable
	{
		public Station SourceStation { get; set; }

		public Station DestinationStation { get; set; }

		public DateTime? Date { get; set; }

		public Train Train { get; set; }
		
		public IDictionary<string, string> ToDictionary(IDictionary<string, string> src = null)
		{
			if (src == null)
			{
				src = new Dictionary<string, string>();
			}

			src["from"] = (SourceStation?.ID ?? Train.From.Code).ToString();
			src["to"] =  (DestinationStation?.ID ?? Train.To.Code).ToString();
			src["date"] = (Date ?? Train.Departure ?? DateTime.Now).ToSortableDateString();
			src["train"] = Train.Number;

			return src;
		}

		public static RouteData Create(Station sourceStation, Station destinationStation, DateTime date, Train train)
		{
			return new RouteData
					{
						SourceStation = sourceStation,
						DestinationStation = destinationStation,
						Date = date,
						Train = train
					};
		}
	}
}
