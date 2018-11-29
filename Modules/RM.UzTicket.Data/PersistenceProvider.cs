using System.Threading.Tasks;
using RedisBoost;
using RM.UzTicket.Data.Contracts;
using RM.UzTicket.Settings.Contracts;

namespace RM.UzTicket.Data
{
	internal class PersistenceProvider : IPersistenceProvider
	{
		private const string _baseNamespace = "uzticket";

		private readonly ISettingsProvider _settingProvider;
		private readonly IPersistenceSettings _persistenceSettings;
		private readonly IRedisClientsPool _clientPool;

		public PersistenceProvider(ISettingsProvider settingProvider)
		{
			_settingProvider = settingProvider;
			_persistenceSettings = _settingProvider.GetSettings().Persistence;
			_clientPool = RedisClient.CreateClientsPool(4, 20 * 60 * 1000);
		}

		public async Task<IPersistenceClient> GetClientAsync(string dataNamespace)
		{
			var builder = new RedisConnectionStringBuilder(_persistenceSettings.ConnectionString);
			var redisClient = await _clientPool.CreateClientAsync(builder.EndPoint, serializer: new DataSerializer());

			if (!redisClient.IsAuthenticated)
			{
				await redisClient.AuthAsync(_persistenceSettings.DatabasePassword);
			}

			if (builder.DbIndex > 0)
			{
				await redisClient.SelectAsync(builder.DbIndex);
			}

			return new PersistenceClient(redisClient, PersistenceClient.GetFullKey(_baseNamespace, dataNamespace));
		}
	}
}
