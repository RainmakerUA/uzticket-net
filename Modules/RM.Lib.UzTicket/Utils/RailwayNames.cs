using System.Collections.Generic;

namespace RM.Lib.UzTicket.Utils
{
    public static class RailwayNames
    {
	    private static readonly IDictionary<int, IDictionary<bool, string>> _names;

	    static RailwayNames()
	    {
			_names = new Dictionary<int, IDictionary<bool, string>>
						{
							//Railways: 32: П-Зах., 35: Льв., 40: Од., 43: Півд., 45: Придн., 48: Дон.
							{ 32, new Dictionary<bool, string> { { false, "П-Зах." }, { true, "Південно-Західна" } } },
							{ 35, new Dictionary<bool, string> { { false, "Льв." }, { true, "Львівська" } } },
							{ 40, new Dictionary<bool, string> { { false, "Од." }, { true, "Одеська" } } },
							{ 43, new Dictionary<bool, string> { { false, "Півд." }, { true, "Південна" } } },
							{ 45, new Dictionary<bool, string> { { false, "Придн." }, { true, "Придніпровська" } } },
							{ 48, new Dictionary<bool, string> { { false, "Дон." }, { true, "Донецька" } } },
						};
	    }

	    public static string GetName(int id, bool longName)
	    {
			return _names.TryGetValue(id, out var nameDict)
						? nameDict.TryGetValue(longName, out var name) ? name : null
						: null;
	    }
    }
}
