using System;

namespace RM.UzTicket.Lib.Exceptions
{
	public class ScanNotFoundException : Exception
	{
		public ScanNotFoundException(string scanId)
		{
			ScanId = scanId;
		}

		public ScanNotFoundException(string message, string scanId) : base(message)
		{
			ScanId = scanId;
		}

		public string ScanId { get; }
	}
}
