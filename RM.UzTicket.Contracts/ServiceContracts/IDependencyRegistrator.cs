namespace RM.UzTicket.Contracts.ServiceContracts
{
	public interface IDependencyRegistrator
	{
		void Register(IDependencyContainer container, IDependencyResolver resolver);
	}
}
