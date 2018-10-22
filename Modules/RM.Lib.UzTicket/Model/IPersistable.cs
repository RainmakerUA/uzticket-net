using System.Collections.Generic;

namespace RM.UzTicket.Lib.Model
{
	public interface IPersistable
	{
		IDictionary<string, string> ToDictionary(IDictionary<string, string> src = null);
	}
}
