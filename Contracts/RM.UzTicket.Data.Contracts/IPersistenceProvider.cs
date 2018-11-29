using System.Threading.Tasks;

namespace RM.UzTicket.Data.Contracts
{
	public interface IPersistenceProvider
	{
		Task<IPersistenceClient> GetClientAsync(string dataNamespace);
	}
}
