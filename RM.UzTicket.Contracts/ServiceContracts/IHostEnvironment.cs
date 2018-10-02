
namespace RM.UzTicket.Contracts.ServiceContracts
{
	public interface IHostEnvironment
	{
		IDependencyResolver Resolver { get; }

		IDependencyContainer Container { get; }
	}
}