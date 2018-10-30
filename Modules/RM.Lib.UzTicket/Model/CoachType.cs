using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RM.Lib.UzTicket.Model
{
	[DataContract]
	public class CoachType : IPersistable
	{
		[DataMember(IsRequired = true)]
		public string Title { get; set; }

		[DataMember(IsRequired = true)]
		public string Letter { get; set; }

		[DataMember(Name = "places")]
		public int PlacesCount { get; set; }

		public override string ToString()
		{
			return $"{Letter}: {PlacesCount} ({Title})";
		}

		public IDictionary<string, string> ToDictionary(IDictionary<string, string> src = null)
		{
			if (src == null)
			{
				src = new Dictionary<string, string>();
			}

			src["wagon_type_id"] = Letter;

			return src;
		}

		internal static CoachType Create(string title, string letter, int placesCount = 0)
		{
			return new CoachType
						{
							Title = title,
							Letter = letter,
							PlacesCount = placesCount
						};
		}
	}
}
