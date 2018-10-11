using RM.Lib.Hosting.Contracts;
using RM.UzTicket.Settings.Contracts;

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
