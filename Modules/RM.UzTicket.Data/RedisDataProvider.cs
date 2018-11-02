using System.Threading.Tasks;
using RedisBoost;
using RM.UzTicket.Settings.Contracts;

namespace RM.UzTicket.Data
{
	public class RedisDataProvider
	{
		private const string _baseNamespace = "uzticket";

		private readonly ISettingsProvider _settingProvider;
		private readonly IPersistenceSettings _persistenceSettings;

		private readonly string _namespace;
		private IRedisClient _client;

		public RedisDataProvider(ISettingsProvider settingProvider)
		{
			_settingProvider = settingProvider;
			_persistenceSettings = _settingProvider.GetSettings().Persistence;
			
		}

		public async Task ConnectAsync()
		{
			_client = await RedisClient.ConnectAsync(_persistenceSettings.ConnectionString);
			var authRes = await _client.AuthAsync(_persistenceSettings.DatabasePassword);
		}

		public async Task<string[]> GetProxyItemsAsync()
		{
			var key = "uzticket:proxy:list";
			var len = await _client.LLenAsync(key);
			return (await _client.LRangeAsync(key, 0, (int)len - 1)).AsArray<string>();
		}
	}
}
