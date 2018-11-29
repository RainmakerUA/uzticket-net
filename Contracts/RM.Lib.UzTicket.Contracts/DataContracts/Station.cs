using System.Runtime.Serialization;

namespace RM.Lib.UzTicket.Contracts.DataContracts
{
	[DataContract]
	public class Station
	{
		[DataMember(Name = "value", IsRequired = true)]
		public int ID { get; set; }

		[DataMember(IsRequired = true)]
		public string Title { get; set; }

		[DataMember(IsRequired = false)]
		public string Region { get; set; }

		[DataMember(IsRequired = false)]
		public string Country { get; set; }

		public override string ToString()
		{
			return $"{Title} ({ID})";
		}
	}
}
