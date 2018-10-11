namespace RM.Lib.Hosting.Contracts
{
	public interface IDependencyRegistrator
	{
		void Register(IDependencyContainer container, IDependencyResolver resolver);
	}
}
