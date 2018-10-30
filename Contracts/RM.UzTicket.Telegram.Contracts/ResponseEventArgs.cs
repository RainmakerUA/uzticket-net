using System;
using System.Collections.Generic;
using System.Text;

namespace RM.UzTicket.Telegram.Contracts
{
	public class ResponseEventArgs : BotEventArgs
	{
		public ResponseEventArgs(long sender, int messageID, string data)
				: base(sender)
		{
			MessageID = messageID;
			Data = data;
		}

		public int MessageID { get; }

		public string Data { get; }
	}
}
