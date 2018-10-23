using System.Runtime.Serialization;

namespace RM.Lib.UzTicket.Model
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

		public override string ToString()
		{
			return $"{Title} ({ID})";
		}

		internal static Station Create(int id, string title, string region = null)
		{
			return new Station { ID = id, Title = title, Region = region };
		}
	}
}
