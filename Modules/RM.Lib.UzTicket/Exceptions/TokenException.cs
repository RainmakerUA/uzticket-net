using System;

namespace RM.Lib.UzTicket.Exceptions
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
