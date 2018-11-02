using System;
using System.Runtime.Serialization;

namespace RM.Lib.UzTicket.Contracts.DataContracts
{
	[DataContract]
	public class ScanItem
	{
		[DataMember(IsRequired = false)]
		public string ScanSource { get; set; }

		[DataMember(IsRequired = false)]
		public long? CallbackID { get; set; }

		[DataMember(IsRequired = true)]
		public string FirstName { get; set; }

		[DataMember(IsRequired = true)]
		public string LastName { get; set; }

		[DataMember(IsRequired = true)]
		public DateTime Date { get; set; }

		[DataMember(IsRequired = true)]
		public Station Source { get; set; }

		[DataMember(IsRequired = true)]
		public Station Destination { get; set; }

		[DataMember(IsRequired = true)]
		public string TrainNumber { get; set; }

		[DataMember(IsRequired = false)]
		public string CoachType { get; set; }

		[DataMember(IsRequired = false)]
		public bool ExactCoachType { get; set; }
	}
}
