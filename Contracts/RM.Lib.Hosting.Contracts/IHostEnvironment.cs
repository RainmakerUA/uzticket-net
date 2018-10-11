
namespace RM.Lib.Hosting.Contracts
{
	public interface IHostEnvironment
	{
		IDependencyResolver Resolver { get; }

		IDependencyContainer Container { get; }
	}
}