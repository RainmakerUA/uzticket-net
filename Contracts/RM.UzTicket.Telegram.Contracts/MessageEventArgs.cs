using System;

namespace RM.UzTicket.Telegram.Contracts
{
	public sealed class MessageEventArgs : BotEventArgs
	{
		public MessageEventArgs(long sender, bool isMaster, string message) : base(sender, isMaster)
		{
			Message = message;
		}

		public string Message { get; }
	}
}
