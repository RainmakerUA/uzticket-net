using RM.UzTicket.Contracts.ServiceContracts;

namespace RM.UzTicket.Hosting
{
	public interface IHostStartWarmup
	{
		void Warmup(IHostEnvironment environment);
	}
}