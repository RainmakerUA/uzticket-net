using RM.UzTicket.Contracts.ServiceContracts;

namespace RM.UzTicket.Telegram.Properties
{
	public class DependencyRegistrator : DependencyRegistratorBase, IDependencyRegistrator
	{
		public void Register(IDependencyContainer container, IDependencyResolver resolver)
		{
			container.RegisterSingletonType<ITelegramBot, TelegramBot>();

			LoadModuleConfig(container);
			RegisterModuleInitializer(resolver, TelegramBot.Initialize);
		}
	}
}
