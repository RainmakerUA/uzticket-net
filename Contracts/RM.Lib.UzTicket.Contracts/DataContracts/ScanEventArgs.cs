using System;

namespace RM.Lib.UzTicket.Contracts.DataContracts
{
	public sealed class ScanEventArgs : EventArgs
	{
		public enum ScanType
		{
			None = 0,
			Success,
			Warning,
			Error
		}

		public ScanEventArgs(long? callbackID, ScanType type, string message)
		{
			CallbackID = callbackID;
			Type = type;
			Message = message;
		}

		// public EventType ?

		public long? CallbackID { get; }

		public ScanType Type { get; }

		public string Message { get; }
	}
}
