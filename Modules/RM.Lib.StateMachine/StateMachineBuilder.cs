using System;
using System.Linq.Expressions;
using RM.Lib.StateMachine.Contracts;

namespace RM.Lib.StateMachine
{
	internal class StateMachineBuilder<TState, TImpl, TInput> : IStateMachineBuilder<TState, TImpl, TInput> where TState : struct, Enum where TImpl : class
	{
		private readonly IStateMachineBuilder<TState, TImpl, TInput> _builder;

		public StateMachineBuilder()
		{
			_builder = Machine<TState, TImpl, TInput>.GetBuilder();
		}

		public IStateMachineBuilder<TState, TImpl, TInput> AddState(TState stateValue, Expression<Action<TImpl, TState, TInput>> onEnter, Expression<Action<TImpl, TState, TInput>> onTransitionError, Expression<Action<TImpl, TState, TInput>> onLeave)
		{
			return _builder.AddState(stateValue, onEnter, onTransitionError, onLeave);
		}

		public IStateMachineBuilder<TState, TImpl, TInput> AddStates((TState stateValue, Expression<Action<TImpl, TState, TInput>> onEnter, Expression<Action<TImpl, TState, TInput>> onTransitionError, Expression<Action<TImpl, TState, TInput>> onLeave)[] states)
		{
			return _builder.AddStates(states);
		}

		public IStateMachineBuilder<TState, TImpl, TInput> AddTransition(TState fromState, TState toState, Expression<Func<TImpl, TState, TState, TInput, bool>> condition)
		{
			return _builder.AddTransition(fromState, toState, condition);
		}

		public IStateMachineBuilder<TState, TImpl, TInput> AddTransitions((TState fromState, TState toState, Expression<Func<TImpl, TState, TState, TInput, bool>> condition)[] transitions)
		{
			return _builder.AddTransitions(transitions);
		}

		public IStateMachine<TState, TImpl, TInput> Build(TImpl implementation)
		{
			return _builder.Build(implementation);
		}
	}
}
