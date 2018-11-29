using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RedisBoost.Core.Serialization;

namespace RM.UzTicket.Data
{
	internal sealed class DataSerializer : BasicRedisSerializer
	{
		private static readonly JsonSerializer _serializer;

		static DataSerializer()
		{
			var settings = new JsonSerializerSettings
								{
									ContractResolver = new CamelCasePropertyNamesContractResolver()
								};
			_serializer = JsonSerializer.CreateDefault(settings);
		}

		protected override byte[] SerializeComplexValue(Type type, object value)
		{
			return GetBytes(JObject.FromObject(value, _serializer).ToString(Formatting.None));
		}

		protected override object DeserializeComplexValue(Type type, byte[] value)
		{
			var token = JToken.Parse(GetString(value));
			return token.ToObject(type, _serializer);
		}

		private static byte[] GetBytes(string str)
		{
			return Encoding.UTF8.GetBytes(str);
		}

		private static string GetString(byte[] bytes)
		{
			return Encoding.UTF8.GetString(bytes);
		}
	}
}
