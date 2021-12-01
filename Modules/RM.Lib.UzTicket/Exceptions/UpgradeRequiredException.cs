using RM.Lib.UzTicket.Contracts.DataContracts;

namespace RM.Lib.UzTicket.Exceptions
{
	public class UpgradeRequiredException : ResponseException
	{
		public UpgradeRequiredException(string message, UpgradeKind kind, string json = null) : base(message, json)
		{
			Kind = kind;
		}

		public UpgradeKind Kind { get; }
	}
}
