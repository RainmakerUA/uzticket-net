using System;

namespace RM.Lib.StateMachine.Contracts
{
	public interface IStateMachine<TState, TImpl, TInput> where TState : struct, Enum where TImpl : class
	{
		TState? CurrentState { get; }

		bool IsFinished { get; }

		TImpl Model { get; }

		bool MoveNext(TInput input);

		void Reset();
	}
}
