using RM.Lib.Hosting.Contracts;
using RM.Lib.UzTicket.Contracts;

namespace RM.Lib.UzTicket.Properties
{
    public class DependencyRegistrator : DependencyRegistratorBase, IDependencyRegistrator
    {
        public void Register(IDependencyContainer container, IDependencyResolver resolver)
        {
            container.RegisterSingletonType<IUzClient, UzClient>();

            LoadModuleConfig(container);
            RegisterModuleInitializer(resolver, UzClient.Initialize);

            //RegisterModuleInitializer<ConfigSection>(resolver, InitWithConfig);
            //RegisterModuleInitializer<BooSection>(resolver, InitWithConfigs);
        }

        private static void InitWithConfig(IDependencyResolver resolver, ConfigSection section)
        {

        }

        private static void InitWithConfigs(IDependencyResolver resolver, BooSection[] section)
        {

        }
    }
}