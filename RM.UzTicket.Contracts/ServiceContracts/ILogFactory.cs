using System;

namespace RM.UzTicket.Contracts.ServiceContracts
{
	public interface ILogFactory
	{
		ILog GetLog();

		ILog GetLog(Type type);
		
		ILog GetLog(string name);
	}
}
