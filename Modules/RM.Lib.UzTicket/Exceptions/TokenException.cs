using System;

namespace RM.UzTicket.Lib.Exceptions
{
	public class TokenException : UzException
	{
		public TokenException()
		{
		}

		public TokenException(string message) : base(message)
		{
		}

		public TokenException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
