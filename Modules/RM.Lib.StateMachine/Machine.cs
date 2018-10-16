using System;
using System.Collections.Generic;
using System.Linq;
using RM.Lib.StateMachine.Contracts;
using RM.Lib.Utility;

namespace RM.Lib.StateMachine
{
	internal sealed partial class Machine<TState, TImpl, TInput> : IStateMachine<TState, TImpl, TInput>
				where TState : struct, Enum where TImpl : class
	{
		private readonly IDictionary<TState, State> _states;

		private readonly TransitionCollection _transitions;

		private readonly State _initialState;

		private State? _currentState;

		private Machine(TImpl implementation, Builder builder)
		{
			Model = implementation;
			_states = GetStates(builder, implementation);
			_transitions = new TransitionCollection(GetTransitions(builder, implementation));

			var initial = builder.InitialState ?? GetZeroValue();

			_initialState = _states.TryGetValue(initial, out var zeroState)
								? zeroState
								: throw new ArgumentException("State machine must contain initial state with field value = 0");
			Reset();
		}

		public TState? CurrentState => _currentState?.StateValue;

		public bool IsFinished { get; private set; }

		public TImpl Model { get; }

		public bool MoveNext(TInput input)
		{

			if (!_currentState.HasValue)
			{
				throw new InvalidOperationException("State machine has no current state!");
			}

			var currentState = _currentState.Value.StateValue;

			if (!IsFinished)
			{
				Transition? availableTransition = null;
				Transition? conditionlessTransition = null;

				foreach (var trans in _transitions[currentState])
				{
					if (!conditionlessTransition.HasValue && trans.Condition == null)
					{
						conditionlessTransition = trans;
					}
					else if (trans.CheckCondition(input))
					{
						availableTransition = trans;
						break;
					}
				}

				var transition = availableTransition ?? conditionlessTransition;

				if (transition is Transition selectedTran)
				{
					var newStateValue = selectedTran.ToState;
					var newState = _states[newStateValue];

					_currentState.Value.OnLeave?.Invoke(currentState, input);
					newState.OnEnter?.Invoke(newStateValue, input);
					_currentState = newState;

					if (_transitions[newStateValue].Count == 0)
					{
						IsFinished = true;
					}

					return true;
				}
			}

			_currentState?.OnTransitionError?.Invoke(currentState, input);

			return false;
		}

		public void Reset()
		{
			_currentState = _initialState;
		}

		internal static IStateMachineBuilder<TState, TImpl, TInput> GetBuilder()
		{
			return new Builder();
		}

		private static TState GetZeroValue()
		{
			var enumType = typeof(TState);

			if (Enum.IsDefined(enumType, 0))
			{
				return (TState)Enum.ToObject(enumType, 0);
			}

			throw new ArgumentException("TState enum type must have zero field defined!");
		}

		private static IDictionary<TState, State> GetStates(Builder builder, TImpl impl)
		{
			return builder.States.Select(t => new State(t.stateValue, t.onEnter?.SetReceiver(impl), t.onTransitionError?.SetReceiver(impl), t.onLeave?.SetReceiver(impl)))
									.ToDictionary(s => s.StateValue);
		}

		private static IEnumerable<Transition> GetTransitions(Builder builder, TImpl impl)
		{
			return builder.Transitions.Select(t => new Transition(t.fromState, t.toState, t.condition?.SetReceiver(impl)));
		}
	}
}
