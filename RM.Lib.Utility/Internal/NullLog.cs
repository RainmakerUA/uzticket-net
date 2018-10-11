using System;
using RM.Lib.Common.Contracts.Log;

namespace RM.Lib.Utility.Internal
{
	internal sealed class NullLog : ILog
	{
		public bool IsTraceEnabled { get; private set; }

		public void Trace(string message, params object[] arguments) { }

		public void Trace(string message, Exception exception, params object[] arguments) { }

		public bool IsDebugEnabled { get; private set; }

		public void Debug(string message, params object[] arguments){}

		public void Debug(string message, Exception exception, params object[] arguments){}

		public bool IsInfoEnabled { get; private set; }

		public void Info(string message, params object[] arguments){}

		public void Info(string message, Exception exception, params object[] arguments){}

		public bool IsWarnEnabled { get; private set; }

		public void Warning(string message, params object[] arguments){}

		public void Warning(string message, Exception exception, params object[] arguments){}

		public bool IsErrorEnabled { get; private set; }

		public void Error(string message, params object[] arguments){}

		public void Error(string message, Exception exception, params object[] arguments){}

		public bool IsFatalEnabled { get; private set; }

		public void Fatal(string message, params object[] arguments){}

		public void Fatal(string message, Exception exception, params object[] arguments){}
	}
}