
namespace RM.UzTicket.Utility.StateMachine
{
	public interface IStateTransition<T> where T : struct
	{
		T From { get; }

		T To { get; }

		MoveToAction<T> MoveCallback { get; }
	}
}
