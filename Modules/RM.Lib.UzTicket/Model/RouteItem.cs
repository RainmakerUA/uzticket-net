using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace RM.UzTicket.Lib.Model
{
	[DataContract]
	public class RouteItem
	{
		[DataMember(IsRequired = true)]
		public int Code { get; set; }

		[DataMember(IsRequired = true)]
		public string Name { get; set; }

		[DataMember(Name = "lat")]
		public double Lattitude { get; set; }

		[DataMember(Name = "long")]
		public double Longitude { get; set; }

		[DataMember]
		[JsonConverter(typeof(Utils.TimeSpanConverter))]
		public TimeSpan ArrivalTime { get; set; }

		[DataMember]
		[JsonConverter(typeof(Utils.TimeSpanConverter))]
		public TimeSpan DepartureTime { get; set; }

		[DataMember]
		public int Distance { get; set; }

		internal bool? IsFinal { get; set; }

		public override string ToString()
		{
			var timeStr = IsFinal.HasValue
							? (IsFinal.Value ? $"{ArrivalTime,-13:hh\\:mm}" : $"{DepartureTime,13:hh\\:mm}")
							: $"{ArrivalTime:hh\\:mm} - {DepartureTime:hh\\:mm}";
			return $"{Distance,4} {timeStr} {Name}";
		}

		// Gmaps link: https://www.google.com/maps/?q=lat,lng
	}
}
