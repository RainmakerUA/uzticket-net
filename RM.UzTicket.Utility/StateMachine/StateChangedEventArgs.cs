using System;

namespace RM.UzTicket.Utility.StateMachine
{
	public sealed class StateChangedEventArgs<T> : EventArgs where T : struct
	{
		internal StateChangedEventArgs(T from, T to)
		{
			From = from;
			To = to;
		}

		public T From { get; }

		public T To { get; }
	}
}