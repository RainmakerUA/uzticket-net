using RM.UzTicket.Contracts.ServiceContracts;

namespace RM.UzTicket.Proxy.Properties
{
	public sealed class DependencyRegistrator : DependencyRegistratorBase, IDependencyRegistrator
	{
		public void Register(IDependencyContainer container, IDependencyResolver resolver)
		{
			container.RegisterSingletonType<IProxyProvider, ProxyProvider>();

			LoadModuleConfig(container);
		}
	}
}
