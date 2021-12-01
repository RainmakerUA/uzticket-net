using System;
using System.Threading.Tasks;
using RedisBoost;
using RM.Lib.Utility;
using RM.UzTicket.Data.Contracts;

namespace RM.UzTicket.Data
{
	internal sealed class PersistenceClient : IPersistenceClient
	{
		private const string _namespaceSeparator = ":";

		private readonly IRedisClient _client;
		private readonly string _dataNamespace;

		private bool _disposed;

		public PersistenceClient(IRedisClient client, string dataNamespace)
		{
			_client = client;
			_dataNamespace = dataNamespace;
		}

		public void Dispose()
		{
			_client.Dispose();
			_disposed = true;
		}

		public Task<T> GetValueAsync<T>(string key)
		{
			CheckDisposed();

			return _client.GetAsync(GetFullKey(_dataNamespace, key)).Then(bulk => bulk.As<T>());
		}

		public Task SetValueAsync<T>(string key, T value)
		{
			CheckDisposed();

			return _client.SetAsync(GetFullKey(_dataNamespace, key), value);
		}

		public async Task<T[]> GetListAsync<T>(string key, int start = 0, int stop = -1)
		{
			CheckDisposed();

			var len = await _client.LLenAsync(GetFullKey(_dataNamespace, key));
			return len > 0 ? (await _client.LRangeAsync(GetFullKey(_dataNamespace, key), start, stop)).AsArray<T>() : new T[0];
		}

		public async Task<long> SetListAsync<T>(string key, params T[] values)
		{
			CheckDisposed();

			var fullKey = GetFullKey(_dataNamespace, key);
			await _client.MultiAsync();

			try
			{
				await _client.DelAsync(fullKey);
				var result = await _client.RPushAsync(fullKey, values);
				var execResult = (await _client.ExecAsync()).AsArray<string>();
				return result;
			}
			catch (Exception)
			{
				var discardResult = await _client.DiscardAsync();
				throw;
			}
		}

		private void CheckDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(PersistenceClient));
			}
		}

		public static string GetFullKey(string parentNs, string childNs)
		{
			if (String.IsNullOrEmpty(parentNs))
			{
				return childNs;
			}

			if (String.IsNullOrEmpty(childNs))
			{
				return parentNs;
			}

			return parentNs + _namespaceSeparator + childNs;
		}
	}
}
