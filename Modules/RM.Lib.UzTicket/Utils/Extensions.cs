using System;

namespace RM.Lib.UzTicket.Utils
{
	public static class Extensions
	{
		public static string OrDefault(this string str, string defaultStr = null)
		{
			return String.IsNullOrEmpty(str) ? defaultStr : str;
		}

		public static string ToSortableDateString(this DateTime dateTime)
		{
			return dateTime.ToString("yyyy-MM-dd");
		}

		public static void Deconstruct<T>(this T[] array, out T item1, out T item2, out T item3)
		{
			var length = array.Length;

			item1 = length > 0 ? array[0] : default;
			item2 = length > 1 ? array[1] : default;
			item3 = length > 2 ? array[2] : default;
		}
	}
}
