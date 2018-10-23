using System;
using System.Collections.Generic;
using RM.Lib.UzTicket.Utils;

namespace RM.Lib.UzTicket.Model
{
    public class Passenger : IPersistable
    {
		public string FirstName { get; set; }

		public string LastName { get; set; }

		public bool Bedding { get; set; }

		public DateTime ChildBirthDate { get; set; }

		public string StudentID { get; set; }

	    public IDictionary<string, string> ToDictionary(IDictionary<string, string> src = null)
	    {
			if (src == null)
		    {
			    src = new Dictionary<string, string>();
		    }

		    src["firstname"] = FirstName;
		    src["lastname"] = LastName;
		    src["bedding"] = Bedding ? "1" : "0";
		    src["child"] = ChildBirthDate.ToSortableDateString();
		    src["student"] = StudentID;

		    return src;
	}
    }
}
