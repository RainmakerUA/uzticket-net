
namespace RM.UzTicket.Utility.StateMachine
{
	public delegate void MoveToAction<in T>(T from, T to) where T : struct;
}
