using System;

namespace RM.Lib.UzTicket.Exceptions
{
	public class ConfigurationException : UzException
	{
		public ConfigurationException()
		{
		}

		public ConfigurationException(string message) : base(message)
		{
		}

		public ConfigurationException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}