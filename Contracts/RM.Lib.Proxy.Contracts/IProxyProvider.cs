using System;
using System.Threading.Tasks;

namespace RM.Lib.Proxy.Contracts
{
	public interface IProxyProvider
	{
		Task<string> GetProxyAsync(Func<string, Task<bool>> proxyCheckerAsync = null);
	}
}
