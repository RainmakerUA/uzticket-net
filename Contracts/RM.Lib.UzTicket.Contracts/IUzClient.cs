using System.Threading.Tasks;

namespace RM.Lib.UzTicket.Contracts
{
    public interface IUzClient
    {
	    Task<string[]> GetStationsAsync(string name);
    }
}