using System;

namespace RM.UzTicket.Utility.StateMachine
{
	internal struct StateTransition<T> : IStateTransition<T>, IEquatable<StateTransition<T>> where T : struct
	{
		public T From { get; }

		public T To { get; }

		public MoveToAction<T> MoveCallback { get; }

		internal StateTransition(T from, T to, MoveToAction<T> moveCallback)
				: this()
		{
			From = from;
			To = to;
			MoveCallback = moveCallback;
		}

		public override int GetHashCode()
		{
			return From.GetHashCode() ^ To.GetHashCode();
		}

		public bool Equals(StateTransition<T> other)
		{
			return From.Equals(other.From) && To.Equals(other.To);
		}

		public override bool Equals(object obj)
		{
			return obj is StateTransition<T> other && Equals(other);
		}
	}
}
