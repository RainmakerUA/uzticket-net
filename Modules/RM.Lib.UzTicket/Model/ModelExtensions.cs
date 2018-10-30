using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RM.Lib.Common.Contracts.Log;
using RM.Lib.Utility;

namespace RM.Lib.UzTicket.Model
{
	internal static class ModelExtensions
	{
		private static readonly MemoryTraceWriter _tracer;
		private static readonly JsonSerializer _serializer;

		static ModelExtensions()
		{
			_tracer = new MemoryTraceWriter();
			_serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
															{
																ContractResolver = new CamelCasePropertyNamesContractResolver(),
																TraceWriter = _tracer
															});
			_serializer.Error += (sender, args) => Logger.Debug("Deserialization error: " + args.ToString());
		}

		private static ILog Logger => LogFactory.GetLog();

		public static T Deserialize<T>(this JToken jToken)
		{
			return jToken.ToObject<T>(_serializer);
		}

		public static IDictionary<string, string> ToRequestDictionary(this IEnumerable<IPersistable> persistables, IDictionary<string, string> src = null)
		{
			foreach (var persistable in persistables)
			{
				src = persistable.ToDictionary(src);
			}

			return src;
		}

		public static Train GetByNumber(this IEnumerable<Train> trains, string number)
		{
			return trains?.FirstOrDefault(t => StartNumbersEqual(t.Number, number));

			bool StartNumbersEqual(string str1, string str2)
			{
				return String.Equals(str1, str2, StringComparison.InvariantCultureIgnoreCase)
						|| Int32.TryParse(new String(str1.TakeWhile(Char.IsDigit).ToArray()), out int num1)
							&& Int32.TryParse(new String(str2.TakeWhile(Char.IsDigit).ToArray()), out int num2)
					   		&& num1 == num2;
			}
		}

		public static IEnumerable<Seat> GetSeats(this Coach coach, IReadOnlyDictionary<string, int[]> seatNumbers)
		{
			return seatNumbers.Keys.SelectMany(
											charline => seatNumbers[charline],
											(charline, seatNum) => new Seat
																	{
																		CharLine = charline,
																		Number = seatNum,
																		Price = coach.Prices.TryGetValue(charline, out var price) ? price : new decimal?()
																	}
										);
		}

		public static IEnumerable<string> GetTraces()
		{
			return _tracer.GetTraceMessages();
		}
	}
}
