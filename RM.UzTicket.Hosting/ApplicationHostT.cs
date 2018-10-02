namespace RM.UzTicket.Hosting
{
	public class ApplicationHost<THostInitializer> : ApplicationHost<THostInitializer, DefaultHostStartWarmup>
		where THostInitializer : IHostInitializer
	{

		public ApplicationHost()
		{
		}

		public ApplicationHost(THostInitializer initializer)
			: base(initializer, new DefaultHostStartWarmup())
		{
		}
	}

	public class ApplicationHost<THostInitializer, TStartWarmup> : ApplicationHost
		where THostInitializer : IHostInitializer
		where TStartWarmup : IHostStartWarmup
	{
		public ApplicationHost()
			: base(typeof(THostInitializer), typeof(TStartWarmup))
		{
		}

		public ApplicationHost(THostInitializer initializer, TStartWarmup startWarmup)
			: base(initializer, startWarmup)
		{
		}
	}
}
