using System;
using RM.UzTicket.Contracts.ServiceContracts;

namespace RM.UzTicket.Utility.Internal
{
	internal class NullLogFactory : ILogFactory
	{
		public ILog GetLog()
		{
			return new NullLog();
		}

		public ILog GetLog(Type type)
		{
			return new NullLog();
		}

		public ILog GetLog(string name)
		{
			return new NullLog();
		}
	}
}