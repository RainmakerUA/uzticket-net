using RM.UzTicket.Contracts.ServiceContracts;

namespace RM.UzTicket.Settings.Properties
{
	public class DependencyRegistrator : DependencyRegistratorBase, IDependencyRegistrator
	{
		public void Register(IDependencyContainer container, IDependencyResolver resolver)
		{
			container.RegisterSingletonType<ISettingsProvider>(type => SettingsProvider.Load());

			LoadModuleConfig(container);
		}
	}
}
