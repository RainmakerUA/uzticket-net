using System;

namespace RM.UzTicket.Telegram.Contracts
{
	public abstract class BotEventArgs: EventArgs
	{
		protected BotEventArgs(long sender, bool isMaster)
		{
			Sender = sender;
			IsMaster = isMaster;
		}

		public long Sender { get; }

		public bool IsMaster { get; }
	}
}
