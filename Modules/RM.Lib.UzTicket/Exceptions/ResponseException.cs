namespace RM.UzTicket.Lib.Exceptions
{
	public class ResponseException : UzException
	{
		public ResponseException()
		{
		}

		public ResponseException(string message) : base(message)
		{
		}

		public ResponseException(string message, string json) : base(message)
		{
			Json = json;
		}

		public string Json { get; }
	}
}
