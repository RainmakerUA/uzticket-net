using System;

namespace RM.Lib.UzTicket.Contracts.DataContracts
{
	public sealed class ScanEventArgs : EventArgs
	{

		public ScanEventArgs(long? callbackID, ScanEventType type, string message)
		{
			CallbackID = callbackID;
			Type = type;
			Message = message;
		}

		// public EventType ?

		public long? CallbackID { get; }

		public ScanEventType Type { get; }

		public string Message { get; }
	}
}
