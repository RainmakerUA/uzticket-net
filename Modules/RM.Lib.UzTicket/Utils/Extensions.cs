using System;
using System.Threading;
using System.Threading.Tasks;

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

		public static bool WaitedOrCancelled(this CancellationToken token, TimeSpan timeout)
		{
			return !token.IsCancellationRequested && !token.WaitHandle.WaitOne(timeout);
		}

		public static void Deconstruct<T>(this T[] array, out T item1, out T item2)
		{
			var length = array.Length;

			item1 = length > 0 ? array[0] : default;
			item2 = length > 1 ? array[1] : default;
		}

		public static void Deconstruct<T>(this T[] array, out T item1, out T item2, out T item3)
		{
			var length = array.Length;

			item1 = length > 0 ? array[0] : default;
			item2 = length > 1 ? array[1] : default;
			item3 = length > 2 ? array[2] : default;
		}

		public static Task<TRes> Then<T, TRes>(this T task, Func<T, TRes> successFunc, Func<Exception, TRes> failFunc = null) where T : Task
		{
			return task.ContinueWith(t =>
										{
											if (t.IsFaulted)
											{
												var exception = UnwrapAggregate(t.Exception);
												return failFunc != null ? Task.FromResult(failFunc(exception)) : Task.FromException<TRes>(exception);
											}
											return Task.FromResult(successFunc((T)t));
										}).Unwrap();
		}

		private static Exception UnwrapAggregate(Exception ex)
		{
			return ex is AggregateException aggr ? aggr.Flatten().InnerException : ex;
		}
	}
}
