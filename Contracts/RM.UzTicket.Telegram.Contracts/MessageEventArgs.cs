using System;

namespace RM.UzTicket.Telegram.Contracts
{
	public sealed class MessageEventArgs : BotEventArgs
	{
		public MessageEventArgs(long sender, string message) : base(sender)
		{
			Message = message;
		}

		public string Message { get; }
	}
}
