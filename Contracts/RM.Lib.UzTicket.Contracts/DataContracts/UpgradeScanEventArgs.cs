using System.Threading.Tasks;

namespace RM.Lib.UzTicket.Contracts.DataContracts
{
	public sealed class UpgradeScanEventArgs : ScanEventArgs
	{
		public UpgradeScanEventArgs(long? callbackID, UpgradeKind kind, string dataType, byte[] data)
				: base(callbackID, ScanEventType.Warning, null)
		{
			Kind = kind;
			DataType = dataType;
			Data = data;
		}

		public UpgradeKind Kind { get; }

		public string DataType { get; }

		public byte[] Data { get; }

		public Task<string> ResponseTask { get; set; }
	}
}
