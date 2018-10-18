using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Linq;
using RM.Lib.StateMachine.Contracts;
using RM.Lib.Utility;

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

			public string MachineName { get; private set; }

			public string ImplementingType { get; private set; }

			public TState? InitialState { get; private set; }

			public IStateMachineBuilder<TState, TImpl, TInput> SetBasicInfo(string name, string implementingType, TState? initialState)
			{
				(MachineName, ImplementingType, InitialState) = (name, implementingType, initialState);
				return this;
			}

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
				Validate(implementation);

				return new Machine<TState, TImpl, TInput>(implementation, this);
			}

			public IStateMachine<TState, TImpl, TInput> BuildFromXml(Stream xmlStream, TImpl implementation)
			{
				var doc = XDocument.Load(xmlStream);
				var root = doc.Root;

				if (root != null)
				{
					SetBasicInfo(
								root.Attribute("name")?.Value,
								root.Element(XmlNames.DatamodelName)?.Value,
								Enum.TryParse<TState>(root.Attribute("initial")?.Value ?? String.Empty, true, out var initState)
											? initState
											: default
							);

					foreach (var stateEl in root.Elements(XmlNames.StateName))
					{
						var state = (TState) Enum.Parse(typeof(TState), stateEl.Attribute(XmlNames.Id).Value, true);
						var onEnter = stateEl.Element(XmlNames.OnEntryName)?.Value;
						var onLeave = stateEl.Element(XmlNames.OnExitName)?.Value;
						var onTransError = ParseStateDataModel(stateEl.Element(XmlNames.DatamodelName));

						AddState(
								state,
								onEnter?.MakeActionExpression<TImpl, TState, TInput>(),
								onTransError?.MakeActionExpression<TImpl, TState, TInput>(),
								onLeave?.MakeActionExpression<TImpl, TState, TInput>()
							);

						foreach (var transEl in stateEl.Elements(XmlNames.TransitionName))
						{
							var toState = (TState) Enum.Parse(typeof(TState), transEl.Attribute("target").Value, true);
							var condition = transEl.Attribute("cond")?.Value;

							AddTransition(
										state, toState,
										condition?.MakeFuncExpression<TImpl, TState, TState, TInput, bool>()
									);
						}
					}

					return Build(implementation);
				}
				
				throw new Exception("Error parsing State Machine XML!");
			}

			private void Validate(TImpl implementation)
			{
				if (implementation == null)
				{
					throw new ArgumentNullException(nameof(implementation));
				}

				if (!String.IsNullOrEmpty(ImplementingType))
				{
					var type = Type.GetType(ImplementingType);

					if (type == null)
					{
						throw new StateMachineException($"Implementing type {ImplementingType}");
					}

					if (!type.IsInstanceOfType(implementation))
					{
						throw new StateMachineException($"Implementation instance (of type {implementation.GetType()}) is not of expected type {type.FullName}");
					}
				}

				if (InitialState.HasValue && !IsStateDefined(InitialState.Value))
				{
					throw new StateMachineException($"Initial state {InitialState.Value} is not defined for the state machine!");
				}

				var duplicateState = _states.GroupBy(t => t.stateValue).FirstOrDefault(g => g.Count() > 1)?.Key;

				if (duplicateState.HasValue)
				{
					throw new StateMachineException($"More than one state with value {duplicateState.Value} is defined!");
				}

				var sameStateTransition = GetFirstOrNull(_transitions, tr => tr.HasValue && tr.Value.fromState.Equals(tr.Value.toState))?.fromState;

				if (sameStateTransition.HasValue)
				{
					throw new StateMachineException($"A transition between the same state with value {sameStateTransition.Value} is defined!");
				}

				var (fromState, toState, fromStateUndef, toStateUndef) = _transitions.Select(t => (t.fromState, t.toState, fromStateUndef: !IsStateDefined(t.fromState), toStateUndef: !IsStateDefined(t.toState)))
						.FirstOrDefault(tt => tt.fromStateUndef || tt.toStateUndef);

				if (fromStateUndef)
				{
					throw new StateMachineException($"A transition FROM the undefined state {fromState} exists!");
				}

				if (toStateUndef)
				{
					throw new StateMachineException($"A transition TO the undefined state {toState} exists!");
				}
			}

			private bool IsStateDefined(TState state)
			{
				return _states.FindIndex(t => t.stateValue.Equals(state)) >= 0;
			}

			private static string ParseStateDataModel(XElement dataModelEl)
			{
				if (dataModelEl != null)
				{
					var nodes = dataModelEl.Nodes().ToArray();
					
					if (nodes.Length == 1 && nodes[0].NodeType == XmlNodeType.Text)
					{
						return (nodes[0] as XText)?.Value;
					}

					return nodes.OfType<XElement>().FirstOrDefault(
														xEl => xEl.Name == XmlNames.DataName
																&& "onerror".Equals(xEl.Attribute(XmlNames.Id)?.Value, StringComparison.Ordinal)
													)?.Value
							?? nodes.OfType<XElement>().SingleOrDefault(xEl => xEl.Name == XmlNames.DataName)?.Value;
				}

				return null;
			}

			private static T? GetFirstOrNull<T>(IEnumerable<T> sequence, Func<T?, bool> condition) where T : struct
			{
				return sequence.Cast<T?>().FirstOrDefault(condition);
			}
		}
	}
}
