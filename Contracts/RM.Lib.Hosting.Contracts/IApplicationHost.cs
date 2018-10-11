using System;

namespace RM.Lib.Hosting.Contracts
{
	public interface IApplicationHost : IDisposable
	{
		HostState State { get; }

		IHostEnvironment Environment { get; }

		void Initialize();

		void Start();
		
		void Stop(TimeSpan? waitTime = null);

		event EventHandler Starting;
		
		event EventHandler Started;
		
		event EventHandler Stopping;
		
		event EventHandler Stopped;
	}
}