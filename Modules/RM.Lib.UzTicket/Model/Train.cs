using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using RM.Lib.UzTicket.Utils;

namespace RM.Lib.UzTicket.Model
{
	[DataContract]
	public class Train : IPersistable
	{
		[DataMember(Name = "num", IsRequired = true)]
		public string Number { get; set; }

		[DataMember]
		public int Category { get; set; }

		[DataMember]
		[JsonConverter(typeof(TimeSpanConverter))]
		public TimeSpan TravelTime { get; set; }

		[DataMember(Name = "types")]
		public CoachType[] CoachTypes { get; set; }

		[DataMember(IsRequired = true)]
		public TrainStation From { get; set; }

		[DataMember(IsRequired = true)]
		public TrainStation To { get; set; }

		public string SourceStationName => From?.Station;

		public string DestinationStationName => To?.Station;

		public string TrainSourceStationName => From?.TerminalStation;

		public string TrainDestinationStationName => To?.TerminalStation;

		public DateTime? Departure => From?.TrainTime;

		public DateTime? Arrival => To?.TrainTime;

		public override string ToString()
		{
			return $"{Number}: {TrainSourceStationName} - {TrainDestinationStationName}";
		}

		public IDictionary<string, string> ToDictionary(IDictionary<string, string> src = null)
		{
			if (src == null)
			{
				src = new Dictionary<string, string>();
			}

			src["from"] = From.Code.ToString();
			src["to"] = To.Code.ToString();
			src["train"] = Number;
			src["date"] = Departure.GetValueOrDefault().ToSortableDateString();

			return src;
		}

		public string GetInfo()
		{
			var list = new List<string>
							{
								"Train: {0} {1} - {2}",
								"Departure time: {3}",
								"Arrival time: {4}",
								"Travel time: {5}",
								"~~~~~~~~~~~~~~~~~~"
							};
			list.AddRange(CoachTypes.Select(ct => ct.ToString()));
			return String.Format(
							String.Join(Environment.NewLine, list),
							Number, From.TerminalStation, To.TerminalStation,
							Departure, Arrival, TravelTime
						);
		}

		internal static Train Create(string number, Station from, Station to, DateTime departure, DateTime arrival, CoachType[] types)
		{
			return new Train
						{
							Number = number,
							//TODO
							//SourceStation = from,
							//DestinationStation = to,
							//DepartureTime = UzTime.Create(departure),
							//ArrivalTime = UzTime.Create(arrival),
							//TrainSourceStation = $"Before_{from.Title}",
							//TrainDestinationStation = $"After_{to.Title}",
							TravelTime = arrival - departure,
							CoachTypes = types
						};
		}
	}
}
