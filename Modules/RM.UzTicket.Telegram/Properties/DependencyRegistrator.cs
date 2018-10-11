using RM.Lib.Hosting.Contracts;
using RM.UzTicket.Telegram.Contracts;

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
