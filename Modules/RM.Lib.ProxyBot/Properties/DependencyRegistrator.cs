using RM.Lib.Hosting.Contracts;
using RM.Lib.Proxy.Contracts;

namespace RM.Lib.ProxyBot.Properties
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
