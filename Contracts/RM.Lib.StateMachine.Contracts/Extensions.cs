using System;
using System.Linq;
using System.Linq.Expressions;

namespace RM.Lib.StateMachine.Contracts
{
	public static class Extensions
	{
		public static IStateMachineBuilder<TState, TImpl, TInput> AddDefaultStates<TState, TImpl, TInput>(this IStateMachineBuilder<TState, TImpl, TInput> builder,
																											Expression<Action<TImpl, TState, TInput>> enter,
																											Expression<Action<TImpl, TState, TInput>> error,
																											Expression<Action<TImpl, TState, TInput>> leave)
				where TState : struct, Enum where TImpl : class
		{
			return builder.AddStates(Enum.GetValues(typeof(TState)).Cast<TState>().Select(state => (state, enter, error, leave)).ToArray());
		}
	}
}
