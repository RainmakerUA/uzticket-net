using RM.Lib.Hosting.Contracts;
using RM.Lib.StateMachine.Contracts;

namespace RM.Lib.StateMachine.Properties
{
	public sealed class DependencyRegistrator : DependencyRegistratorBase, IDependencyRegistrator
	{
		public void Register(IDependencyContainer container, IDependencyResolver resolver)
		{
			container.RegisterType(typeof(IStateMachineBuilder<,,>), typeof(StateMachineBuilder<,,>));

			LoadModuleConfig(container);
		}
	}
}
