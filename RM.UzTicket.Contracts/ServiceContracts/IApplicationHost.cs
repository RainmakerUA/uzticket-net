using System;
using RM.UzTicket.Contracts.DataContracts;

namespace RM.UzTicket.Contracts.ServiceContracts
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