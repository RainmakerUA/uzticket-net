namespace RM.Lib.Hosting.Contracts
{
	public interface IHostStartWarmup
	{
		void Warmup(IHostEnvironment environment);
	}
}