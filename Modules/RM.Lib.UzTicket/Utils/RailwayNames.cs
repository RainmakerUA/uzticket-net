using System.Collections.Generic;

namespace RM.Lib.UzTicket.Utils
{
    public static class RailwayInfo
    {
	    private static readonly IDictionary<int, (string, string)> _names;

	    private static readonly IDictionary<int, (string, string)> _countries;

	    static RailwayInfo()
	    {
			_names = new Dictionary<int, (string, string)>
							{
								{ 32, ("П-Зах.", "Південно-Західна") },
								{ 35, ("Льв.", "Львівська") },
								{ 40, ("Од.", "Одеська") },
								{ 43, ("Півд.", "Південна") },
								{ 45, ("Придн.", "Придніпровська") },
								{ 48, ("Дон.", "Донецька") },
							};

			_countries = new Dictionary<int, (string, string)>
							{
								{ 20, ("Russia", null) },
								{ 21, ("Belarus", null) },
								{ 22, ("Ukraine", null) },
								{ 23, ("Moldova", null) },
								{ 24, ("Lithuania", null) },
								{ 25, ("Latvia", null) },
								{ 26, ("Estonia", null) },
								{ 27, ("Kazakhstan", null) },
								{ 28, ("Georgia", null) },
								{ 29, ("Uzbekistan", null) },
								{ 31, ("Mongolia", null) },
								{ 57, ("Azerbaijan", null) },
								{ 58, ("Armenia", null) },
								{ 59, ("Kyrgyzstan", null) },
								{ 66, ("Tajikistan", null) },
								{ 67, ("Turkmenistan", null) }
							};
	    }

	    public static string GetName(int id, bool longName)
	    {
			return _names.TryGetValue(id, out var name)
						? longName ? name.Item2 : name.Item1
						: null;
	    }

	    public static string GetStationCountry(int code, bool emoji)
	    {
		    var countryCode = code / 10000;
		    return _countries.TryGetValue(countryCode, out var country)
						? emoji ? country.Item2 : country.Item1
						: null;
	    }
    }
}
