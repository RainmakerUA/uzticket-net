using System.Collections.Generic;
using RM.Lib.Hosting.Contracts;
using RM.Lib.UzTicket.Contracts;
using RM.Lib.UzTicket.Contracts.DataContracts;
using RM.UzTicket.Telegram.Contracts;

namespace RM.UzTicket.Bot
{
	internal static class MainModule
	{
		private static readonly IDictionary<ScanEventArgs.ScanType, string> _headers = new Dictionary<ScanEventArgs.ScanType, string>
																							{
																								[ScanEventArgs.ScanType.Success] = "\u2705 Scan success!",
																								[ScanEventArgs.ScanType.Warning] = "\u2757 Scan warning.",
																								[ScanEventArgs.ScanType.Error] = "\u274C Scan error!"
																							};

		public static void Initialize(IDependencyResolver resolver)
		{
			var uzClient = resolver.Get<IUzClient>();
			var teleClient = resolver.Get<ITelegramBot>();

			uzClient.ScanEvent += (o, e) =>
			{
				var header = _headers.TryGetValue(e.Type, out var scanHeader) ? scanHeader : null;
				teleClient.SendMessageAsync(e.CallbackID ?? 0, header == null ? e.Message : header + "\n" + e.Message);
			};

			teleClient.Command += async (o, e) =>
			{
				switch (e.Command)
				{
					case Command.Reset:
						await uzClient.ResetScan();
						break;
				}
			};
		}
	}
}
