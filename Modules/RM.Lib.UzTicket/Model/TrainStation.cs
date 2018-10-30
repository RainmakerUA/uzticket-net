using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RM.Lib.UzTicket.Model
{
	[DataContract]
    public class TrainStation
    {
		[DataMember(IsRequired = true)]
		public int Code { get; set; }

		[DataMember(Name = "station", IsRequired = true)]
		public string Station { get; set; }

		[DataMember(Name = "stationTrain")]
		public string TerminalStation { get; set; }

		[DataMember(Name = "sortTime", IsRequired = true)]
		[JsonConverter(typeof(UnixDateTimeConverter))]
		public DateTime TrainTime { get; set; }
    }
}
