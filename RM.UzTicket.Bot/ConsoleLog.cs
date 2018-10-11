using System;
using RM.Lib.Common.Contracts.Log;

namespace RM.UzTicket.Bot
{
	internal sealed class ConsoleLog : ILog
	{
		public bool IsTraceEnabled => true;

		public void Trace(string message, params object[] arguments)
		{
			Trace(message, null, arguments);
		}

		public void Trace(string message, Exception exception, params object[] arguments)
		{
			ConLog("TRACE", message, exception, arguments);
		}

		public bool IsDebugEnabled => true;

		public void Debug(string message, params object[] arguments)
		{
			Debug(message, null, arguments);
		}

		public void Debug(string message, Exception exception, params object[] arguments)
		{
			ConLog("DEBUG", message, exception, arguments);
		}

		public bool IsInfoEnabled => true;

		public void Info(string message, params object[] arguments)
		{
			Info(message, null, arguments);
		}

		public void Info(string message, Exception exception, params object[] arguments)
		{
			ConLog("INFO", message, exception, arguments);
		}

		public bool IsWarnEnabled => true;

		public void Warning(string message, params object[] arguments)
		{
			Warning(message, null, arguments);
		}

		public void Warning(string message, Exception exception, params object[] arguments)
		{
			ConLog("WARN", message, exception, arguments);
		}

		public bool IsErrorEnabled => true;

		public void Error(string message, params object[] arguments)
		{
			Error(message, null, arguments);
		}

		public void Error(string message, Exception exception, params object[] arguments)
		{
			ConLog("ERROR", message, exception, arguments);
		}

		public bool IsFatalEnabled => true;

		public void Fatal(string message, params object[] arguments)
		{
			Fatal(message, null, arguments);
		}

		public void Fatal(string message, Exception exception, params object[] arguments)
		{
			ConLog("FATAL", message, exception, arguments);
		}

		private static void ConLog(string tag, string message, Exception ex, params object[] args)
		{
			Console.WriteLine("{0:yyy-MM-dd hh:mm:ss} [{1}] {2}", DateTime.Now, tag, String.Format(message, args));

			if (ex != null)
			{
				Console.WriteLine(ex);
			}
		}
	}
}
