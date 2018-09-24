using System;
using System.Threading;

namespace RM.UzTicket.Bot.Utils
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

		public void Dispose()
		{
			if (IsCaptured)
			{
				_value.Set();
			}
		}

		void IDisposable.Dispose()
		{
			Dispose();
		}
	}
}
