using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RM.Lib.Hosting;
using RM.Lib.Hosting.Contracts;
using RM.Lib.Utility;

namespace RM.UzTicket.Bot
{
	internal static class Program
	{
		private static async Task Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;

			var host = new ApplicationHost<DefaultHostInitializer>();

			LogFactory.SetDefaultLogFactory(new ConsoleLog());

			//host.Initialize();

			//MainModule.Initialize(host.Environment.Resolver);
			
			//host.Start();

			await MainModule.Test();

			await WaitForCancelKey();

			if (host.State == HostState.Started)
			{
				host.Stop();
			}

			Console.WriteLine("Got shutdown signal. Stopping application...");

#if DEBUG
			Console.WriteLine("Press any key to close...");
			Console.ReadKey(true);
#endif
		}

		private static Task WaitForCancelKey()
		{
			var tcs = new TaskCompletionSource<bool>();

			Console.CancelKeyPress += (o, e) =>
			{
				tcs.SetResult(true);

				if (e.SpecialKey == ConsoleSpecialKey.ControlBreak)
				{
					Console.WriteLine("Terminating app in 10 seconds...");
					Thread.Sleep(TimeSpan.FromSeconds(10));
					Environment.Exit(0);
				}
			};

			return tcs.Task;
		}
	}
}
