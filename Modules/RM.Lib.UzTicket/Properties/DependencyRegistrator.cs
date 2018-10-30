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
        }
    }
}