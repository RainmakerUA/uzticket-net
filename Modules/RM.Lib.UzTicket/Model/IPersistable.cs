using System.Collections.Generic;

namespace RM.Lib.UzTicket.Model
{
	public interface IPersistable
	{
		IDictionary<string, string> ToDictionary(IDictionary<string, string> src = null);
	}
}
