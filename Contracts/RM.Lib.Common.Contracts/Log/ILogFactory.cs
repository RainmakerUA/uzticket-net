using System;

namespace RM.Lib.Common.Contracts.Log
{
	public interface ILogFactory
	{
		ILog GetLog();

		ILog GetLog(Type type);
		
		ILog GetLog(string name);
	}
}
