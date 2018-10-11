using System;

namespace RM.Lib.StateMachine
{
	partial class Machine<TState, TImpl, TInput>
	{
		private struct State
		{
			public State(TState stateValue, Action<TState, TInput> onEnter, Action<TState, TInput> onTransitionError, Action<TState, TInput> onLeave)
			{
				StateValue = stateValue;
				OnEnter = onEnter;
				OnTransitionError = onTransitionError;
				OnLeave = onLeave;
			}

			public TState StateValue { get; }

			public Action<TState, TInput> OnEnter { get; }

			public Action<TState, TInput> OnTransitionError { get; }

			public Action<TState, TInput> OnLeave { get; }
		}
	}
}
