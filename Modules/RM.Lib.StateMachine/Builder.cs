using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using RM.Lib.StateMachine.Contracts;

namespace RM.Lib.StateMachine
{
	partial class Machine<TState, TImpl, TInput>
	{
		private class Builder : IStateMachineBuilder<TState, TImpl, TInput>
		{
			private readonly List<(TState stateValue, Expression<Action<TImpl, TState, TInput>> onEnter, Expression<Action<TImpl, TState, TInput>> onTransitionError, Expression<Action<TImpl, TState, TInput>> onLeave)> _states
								= new List<(TState stateValue, Expression<Action<TImpl, TState, TInput>> onEnter, Expression<Action<TImpl, TState, TInput>> onTransitionError, Expression<Action<TImpl, TState, TInput>> onLeave)>();

			private readonly List<(TState fromState, TState toState, Expression<Func<TImpl, TState, TState, TInput, bool>> condition)> _transitions
								= new List<(TState fromState, TState toState, Expression<Func<TImpl, TState, TState, TInput, bool>> condition)>();

			public IList<(TState stateValue, Expression<Action<TImpl, TState, TInput>> onEnter, Expression<Action<TImpl, TState, TInput>> onTransitionError, Expression<Action<TImpl, TState, TInput>> onLeave)> States => _states;

			public IList<(TState fromState, TState toState, Expression<Func<TImpl, TState, TState, TInput, bool>> condition)> Transitions => _transitions;

			public IStateMachineBuilder<TState, TImpl, TInput> AddState(TState stateValue, Expression<Action<TImpl, TState, TInput>> onEnter, Expression<Action<TImpl, TState, TInput>> onTransitionError, Expression<Action<TImpl, TState, TInput>> onLeave)
			{
				_states.Add((stateValue, onEnter, onTransitionError, onLeave));
				return this;
			}

			public IStateMachineBuilder<TState, TImpl, TInput> AddStates((TState stateValue, Expression<Action<TImpl, TState, TInput>> onEnter, Expression<Action<TImpl, TState, TInput>> onTransitionError, Expression<Action<TImpl, TState, TInput>> onLeave)[] states)
			{
				_states.AddRange(states);
				return this;
			}

			public IStateMachineBuilder<TState, TImpl, TInput> AddTransition(TState fromState, TState toState, Expression<Func<TImpl, TState, TState, TInput, bool>> condition)
			{
				_transitions.Add((fromState, toState, condition));
				return this;
			}

			public IStateMachineBuilder<TState, TImpl, TInput> AddTransitions((TState fromState, TState toState, Expression<Func<TImpl, TState, TState, TInput, bool>> condition)[] transitions)
			{
				_transitions.AddRange(transitions);
				return this;
			}

			public IStateMachine<TState, TImpl, TInput> Build(TImpl implementation)
			{
				//TODO: Validate!
				return new Machine<TState, TImpl, TInput>(implementation ?? throw new ArgumentNullException(nameof(implementation)), this);
			}
		}
	}
}
