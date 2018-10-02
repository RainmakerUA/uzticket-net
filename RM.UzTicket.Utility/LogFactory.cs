using System;
using RM.UzTicket.Contracts.ServiceContracts;
using RM.UzTicket.Utility.Internal;

namespace RM.UzTicket.Utility
{
	public static class LogFactory
	{
		private static readonly object _lockObject = new object();

		private static ILogFactory _logFactory;
		private static ILog _log;

		static LogFactory()
		{
			_logFactory = new NullLogFactory();
			_log = null;
		}

		public static void SetDefaultLog(ILog log)
		{
			if (log == null)
			{
				throw new ArgumentNullException(nameof(log));
			}

			lock (_lockObject)
			{
				_log = log;
			}
		}
		public static void SetDefaultLogFactory(ILogFactory logFactory)
		{
			if (logFactory == null)
			{
				throw new ArgumentNullException(nameof(logFactory));
			}

			lock (_lockObject)
			{
				_logFactory = logFactory;
			}
		}

		public static ILog GetLog()
		{
			return _log ?? _logFactory.GetLog();
		}

		public static ILog GetLog(Type type)
		{
			return _logFactory.GetLog(type);
		}

		public static ILog GetLog(string name)
		{
			return _logFactory.GetLog(name);
		}
	}
}