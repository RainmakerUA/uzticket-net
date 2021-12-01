using System;

namespace RM.Lib.UzTicket.Exceptions
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
