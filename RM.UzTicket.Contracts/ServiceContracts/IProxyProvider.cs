using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RM.UzTicket.Contracts.ServiceContracts
{
	public interface IProxyProvider
	{
		Task<string> GetProxyAsync();
	}
}
