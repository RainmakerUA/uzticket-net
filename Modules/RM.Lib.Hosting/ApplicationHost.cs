using System;
using RM.Lib.Common.Contracts.Log;
using RM.Lib.Hosting.Contracts;
using RM.Lib.Utility;

namespace RM.Lib.Hosting
{
	public class ApplicationHost : IApplicationHost
	{
		private static readonly object _stateLock = new object();

		private readonly Type _initializerType;
		private readonly Type _startWarmupType;
		private readonly IHostInitializer _initializer;
		private readonly IHostStartWarmup _startWarmup;

		private ILog Logger => LogFactory.GetLog();

		public event EventHandler Starting;
		public event EventHandler Started;
		public event EventHandler Stopping;
		public event EventHandler Stopped;

		public HostState State { get; private set; }

		public IHostEnvironment Environment { get; private set; }

		public ApplicationHost()
				: this(typeof(DefaultHostInitializer), typeof(DefaultHostStartWarmup))
		{
		}

		public ApplicationHost(Type initializerType, Type startWarmupType)
		{
			_initializerType = initializerType ?? throw new ArgumentNullException(nameof(initializerType));
			_startWarmupType = startWarmupType ?? throw new ArgumentNullException(nameof(startWarmupType));

			State = HostState.Unknown;
		}

		public ApplicationHost(IHostInitializer initializer)
			: this(initializer, new DefaultHostStartWarmup())
		{
		}

		public ApplicationHost(IHostInitializer initializer, IHostStartWarmup startWarmup)
		{
			_initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
			_startWarmup = startWarmup ?? throw new ArgumentNullException(nameof(startWarmup));

			State = HostState.Unknown;
		}

		public void Initialize()
		{
			Logger.Info("AppHost: initializing");
		
			lock (_stateLock)
			{
				InitializeInternal();
				State = HostState.Initialized;
			}

			Logger.Info("AppHost: initialized");
		}

		public void Start()
		{
			Logger.Info("AppHost: starting");

			Starting?.Invoke(this, EventArgs.Empty);

			lock (_stateLock)
			{
				InternalStart();
				State = HostState.Started;
			}

			Started?.Invoke(this, EventArgs.Empty);

			Logger.Info("AppHost: started");
		}

		public void Stop(TimeSpan? waitTime = null)
		{
			Logger.Info("AppHost: stopping");

			Stopping?.Invoke(this, EventArgs.Empty);

			lock (_stateLock)
			{
				InternalStop(waitTime);
				State = HostState.Initialized;
			}

			Stopped?.Invoke(this, EventArgs.Empty);

			Logger.Info("AppHost: stopped");
		}

		private void InternalStart()
		{
			var warmup = _initializer == null
							? (IHostStartWarmup)Activator.CreateInstance(_startWarmupType)
							: _startWarmup;

			Logger.Info("AppHost: warmup");

			warmup.Warmup(Environment);
		}

		private void InitializeInternal()
		{
			var initializer = _initializer ?? (IHostInitializer)Activator.CreateInstance(_initializerType);

			var env = new HostEnvironment();

			Environment = env;
			initializer.Initialize(env);

			Environment.Container.RegisterInstance<IApplicationHost>(this);

			if (env.Container is DependencyContainer depContainer)
			{
				depContainer.InitializeProvider();
			}
		}

		private void InternalStop(TimeSpan? waitTime = null)
		{
		}

		public void Dispose()
		{
		}
	}
}