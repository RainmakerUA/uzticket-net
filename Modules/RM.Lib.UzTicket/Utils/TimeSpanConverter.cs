using System;
using System.Linq;
using Newtonsoft.Json;

namespace RM.Lib.UzTicket.Utils
{
	internal class TimeSpanConverter : JsonConverter<TimeSpan>
	{
		private static readonly int[] _maxValues = { 0, 59, 59 };

		public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Integer)
			{
				return TimeSpan.FromHours((long)reader.Value);
			}
			
			if (reader.TokenType == JsonToken.Float)
			{
				return TimeSpan.FromHours((double)reader.Value);
			}

			if (reader.TokenType == JsonToken.String)
			{
				var str = (string)reader.Value;
				var parts = str.Split(':');
				var (hours, minutes, seconds) = parts.Select(ParseTimeSpanPart).ToArray();

				return new TimeSpan(hours, minutes, seconds);
			}

			throw new NotSupportedException($"Cannot deserialize TimeSpan from token type {reader.TokenType}");
		}

		public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
		{
			if (value.Ticks < 0)
			{
				throw new NotSupportedException("Negative spans are not supported by converter.");
			}

			var totalHours = (long)Math.Truncate(value.TotalHours);
			var minutes = value.Minutes;

			writer.WriteValue($"{totalHours:D2}:{minutes:D2}");
		}

		private static int ParseTimeSpanPart(string str, int partNum)
		{
			var maxValue = _maxValues[partNum];

			return Int32.TryParse(str, out int val) && val >= 0 && (maxValue == 0 || maxValue >= val)
						? val
						: throw new FormatException($"{str} is not a valid part of a TimeSpan.");
		}
	}
}
