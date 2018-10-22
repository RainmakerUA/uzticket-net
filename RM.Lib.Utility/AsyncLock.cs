using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RM.Lib.Utility
{
	public sealed class AsyncLock : IDisposable
	{
		private static readonly IDictionary<long, AutoResetEvent> _events = new ConcurrentDictionary<long, AutoResetEvent>();
		private readonly AutoResetEvent _event;

		private AsyncLock(object lockObject, bool initialState, int milliseconds)
		{
			_event = lockObject != null
						? GetEvent(lockObject, initialState)
						: throw new ArgumentNullException(nameof(lockObject));
			IsCaptured = _event.WaitOne(milliseconds);
		}

		public bool IsCaptured { get; }

		public void Dispose()
		{
			if (IsCaptured)
			{
				_event.Set();
			}
		}

		public static AsyncLock Lock(object @lock, int milliseconds = Timeout.Infinite)
		{
			return new AsyncLock(@lock, true, milliseconds);
		}

		void IDisposable.Dispose()
		{
			Dispose();
		}

		private static AutoResetEvent GetEvent(object lockObject, bool initialState)
		{
			lock (_events)
			{
				var id = RuntimeHelpers.GetHashCode(lockObject);

				if (!_events.TryGetValue(id, out var @event))
				{
					@event = new AutoResetEvent(initialState);
					_events.Add(id, @event);
				}

				return @event;
			}
		}
	}
}
