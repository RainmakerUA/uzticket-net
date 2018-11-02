
namespace RM.UzTicket.Telegram.Contracts
{
	public class ResponseEventArgs : BotEventArgs
	{
		public ResponseEventArgs(long sender, bool isMaster, int messageID, string data)
				: base(sender, isMaster)
		{
			MessageID = messageID;
			Data = data;
		}

		public int MessageID { get; }

		public string Data { get; }
	}
}
