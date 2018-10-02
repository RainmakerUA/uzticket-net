using System;
using RM.UzTicket.Contracts.ServiceContracts;

namespace RM.UzTicket.Hosting
{
	internal sealed class HostEnvironment : IHostEnvironment
	{
		public IDependencyResolver Resolver { get; }

		public IDependencyContainer Container { get; }

		public HostEnvironment()
		{
			var container = new DependencyContainer();
			Container = container;
			Resolver = container;
		}

		public HostEnvironment(IDependencyContainer container, IDependencyResolver resolver)
		{
			Container = container ?? throw new ArgumentNullException(nameof(container));
			Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
		}
	}
}