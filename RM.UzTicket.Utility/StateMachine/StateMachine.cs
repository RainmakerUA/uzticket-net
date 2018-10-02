using System;
using System.Collections.Generic;
using System.Linq;

namespace RM.UzTicket.Utility.StateMachine
{
	public sealed class StateMachine<T> where T : struct
	{
		private readonly object _globalClassLock = new object();
		private readonly Dictionary<T, ICollection<StateTransition<T>>> _stateTransitions = new Dictionary<T, ICollection<StateTransition<T>>>();

		public event EventHandler<StateChangedEventArgs<T>> StateChanged;

		public T InitialState { get; }

		public T CurrentState { get; private set; }

		public IReadOnlyList<T> NextStates
		{
			get
			{
				lock (_globalClassLock)
				{
					if (!_stateTransitions.TryGetValue(CurrentState, out var transitions))
					{
						return new T[0];
					}

					return transitions.Select(t => t.To).ToArray();
				}
			}
		}

		public IEnumerable<IStateTransition<T>> Transitions
		{
			get
			{
				lock (_globalClassLock)
				{
					return _stateTransitions.SelectMany(t => t.Value).Cast<IStateTransition<T>>().ToArray();
				}
			}
		}

		public StateMachine(T initialState)
		{
			CurrentState = InitialState = initialState;
		}

		public void Reset()
		{
			lock (_globalClassLock)
			{
				CurrentState = InitialState;
			}
		}

		public void Add(T from, T to, MoveToAction<T> moveAction = null)
		{
			if (from.Equals(to))
			{
				throw new Exception(string.Format("One state cycles [{0}-{0}] not allowed.", from));
			}

			lock (_globalClassLock)
			{
				if (_stateTransitions.TryGetValue(from, out var transitions))
				{
					if (transitions.Any(_ => _.To.Equals(to)))
					{
						throw new Exception(string.Format("Transition from {0} to {1} already exists.", from, to));
					}

					transitions.Add(new StateTransition<T>(from, to, moveAction));
				}
				else
				{
					_stateTransitions.Add(from, new List<StateTransition<T>> { new StateTransition<T>(from, to, moveAction) });
				}
			}
		}

		public void MoveTo(T nextState)
		{
			lock (_globalClassLock)
			{
				var transitionExists = false;
				var moveTransition = new StateTransition<T>();
				foreach (var transition in _stateTransitions[CurrentState])
				{
					if (transition.To.Equals(nextState))
					{
						moveTransition = transition;
						transitionExists = true;
						break;
					}
				}

				if (!transitionExists)
				{
					throw new Exception($"Invalid transition: from {CurrentState} to {nextState}");
				}

				moveTransition.MoveCallback?.Invoke(CurrentState, nextState);
				
				var oldState = CurrentState;
				CurrentState = moveTransition.To;

				RaiseStateChanged(oldState, CurrentState);
			}
		}

		private void RaiseStateChanged(T from, T to)
		{
			StateChanged?.Invoke(this, new StateChangedEventArgs<T>(from, to));
		}
	}
}