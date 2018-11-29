using RM.Lib.Hosting.Contracts;
using RM.UzTicket.Data.Contracts;

namespace RM.UzTicket.Data.Properties
{
	public class DependencyRegistrator : DependencyRegistratorBase, IDependencyRegistrator
	{
		public void Register(IDependencyContainer container, IDependencyResolver resolver)
		{
			container.RegisterSingletonType<IPersistenceProvider, PersistenceProvider>();

			LoadModuleConfig(container);
		}
	}
}
