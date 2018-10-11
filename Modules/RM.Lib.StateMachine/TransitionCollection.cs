using System.Collections.Generic;

namespace RM.Lib.StateMachine
{
	partial class Machine<TState, TImpl, TInput>
	{
		private class TransitionCollection
		{
			private readonly IDictionary<TState, List<Transition>> _transitions;

			public TransitionCollection(IEnumerable<Transition> transitions)
			{
				_transitions = CreateDictionary(transitions);
			}

			public IReadOnlyList<Transition> this[TState fromState] => _transitions.TryGetValue(fromState, out var trans)
																			? trans.ToArray()
																			: new Transition[0];

			public Transition? this[TState fromState, TState toState] => _transitions.TryGetValue(fromState, out var trans)
																			? trans.FindIndex(tr => tr.ToState.Equals(toState)) is var index && index >= 0 ? trans[index] : new Transition?()
																			: null;

			private static IDictionary<TState, List<Transition>> CreateDictionary(IEnumerable<Transition> transitions)
			{
				var dict = new Dictionary<TState, List<Transition>>();

				foreach (var transition in transitions)
				{
					var key = transition.FromState;

					if (dict.TryGetValue(key, out var list))
					{
						list.Add(transition);
					}
					else
					{
						dict.Add(key, new List<Transition> { transition });
					}
				}

				return dict;
			}
		}
	}
}
