using System;

namespace RM.Lib.StateMachine
{
	partial class Machine<TState, TImpl, TInput>
	{
		private struct Transition
		{
			public Transition(TState fromState, TState toState, Func<TState, TState, TInput, bool> condition)
			{
				FromState = fromState;
				ToState = toState;
				Condition = condition;
			}

			public TState FromState { get; }

			public TState ToState { get; }

			public Func<TState, TState, TInput, bool> Condition { get; }

			public bool CheckCondition(TInput input)
			{
				return Condition?.Invoke(FromState, ToState, input) ?? true;
			}
		}
	}
}
