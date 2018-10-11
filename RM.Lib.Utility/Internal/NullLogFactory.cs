using System;
using RM.Lib.Common.Contracts.Log;

namespace RM.Lib.Utility.Internal
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