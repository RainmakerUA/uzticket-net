using System;
using System.Threading;

namespace RM.UzTicket.Lib.Utils
{
	internal sealed class AsyncLock : IDisposable
	{
		private readonly AutoResetEvent _value;

		public AsyncLock(AutoResetEvent value, int milliseconds = Timeout.Infinite)
		{
			_value = value ?? throw new ArgumentNullException(nameof(value));
			IsCaptured = value.WaitOne(milliseconds);
		}

		public bool IsCaptured { get; }

		void IDisposable.Dispose()
		{
			if (IsCaptured)
			{
				_value.Set();
			}
		}
	}
}
