using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace RM.Lib.UzTicket.Model
{
	[DataContract]
	public class Route
	{
		[DataMember(Name = "train")]
		public string TrainNumber { get; set; }

		[DataMember(Name = "list")]
		public RouteItem[] Items { get; set; }

		public override string ToString()
		{
			var stations = Items[0].Name + (Items.Length > 2 ? " [...] " : " - ") + Items[Items.Length - 1].Name;
			return $"{TrainNumber} {stations}";
		}

		public string GetInfo()
		{
			var lines = new List<string> { ToString() };
			lines.AddRange(Items.Select(it => it.ToString()));

			return String.Join(Environment.NewLine, lines);
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			Items[0].IsFinal = false;
			Items[Items.Length - 1].IsFinal = true;
		}
	}
}
