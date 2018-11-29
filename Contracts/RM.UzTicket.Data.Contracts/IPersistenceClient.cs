using System;
using System.Threading.Tasks;

namespace RM.UzTicket.Data.Contracts
{
	public interface IPersistenceClient : IDisposable
	{
		Task<T> GetValueAsync<T>(string key);

		Task SetValueAsync<T>(string key, T value);

		Task<T[]> GetListAsync<T>(string key, int start = 0, int stop = -1);

		Task<long> SetListAsync<T>(string key, params T[] values);
	}
}
