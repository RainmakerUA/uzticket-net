using System;
using System.Collections.Generic;
using System.Text;

namespace RM.UzTicket.Telegram.Contracts
{
	public abstract class BotEventArgs: EventArgs
	{
		protected BotEventArgs(long sender)
		{
			Sender = sender;
		}

		public long Sender { get; }
	}
}
