using System;

namespace RM.Lib.UzTicket.Exceptions
{
	public class UzException : ApplicationException
	{
		public UzException()
		{
		}

		public UzException(string message)
			: base(message)
		{
		}

		public UzException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
