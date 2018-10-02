using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using RM.UzTicket.Contracts.ServiceContracts;

namespace RM.UzTicket.Hosting
{
	public class BaseDependencyResolver : IDependencyResolver
	{
		private readonly IServiceScope _scope;
		private IList<Action<IDependencyResolver>> _moduleInitializers;
		protected IServiceProvider Provider;

		private BaseDependencyResolver(IServiceScope scope) : this(scope?.ServiceProvider)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
		}

		protected BaseDependencyResolver(IServiceProvider provider = null)
		{
			if (provider == null)
			{
				_moduleInitializers = new List<Action<IDependencyResolver>>();
			}

			Provider = provider;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void RegisterModuleInitializer(Action<IDependencyResolver> initializer)
		{
			if (_moduleInitializers == null)
			{
				throw new InvalidOperationException("Cannot register module initializer in child resolver or after initialization have taken place!");
			}

			_moduleInitializers.Add(initializer ?? throw new ArgumentNullException(nameof(initializer)));
		}

		public T Get<T>() => Provider.GetRequiredService<T>();

		public IEnumerable<T> GetAll<T>() => Provider.GetServices<T>();

		//public bool IsRegistered<T>()
		//{
		//	return _provider.
		//}

		public object Activate(Type type)
		{
			var constructors = type.GetConstructors();

			foreach (var constructor in constructors)
			{
				var paramInfos = constructor.GetParameters();
				if (paramInfos.Length > 0)
				{
					var parameters = new List<object>();
					var canActivate = true;

					foreach (var paramInfo in paramInfos)
					{
						var parameterType = paramInfo.ParameterType;
						object service;

						if ((service = Get(parameterType)) == null)
						{
							canActivate = false;
							break;
						}

						parameters.Add(service);
					}

					if (canActivate)
					{
						return constructor.Invoke(parameters.ToArray());
					}
				}
				else
				{
					return Activator.CreateInstance(type);
				}
			}

			return null;
		}

		public object Get(Type serviceType) => Provider.GetRequiredService(serviceType);

		public IEnumerable<object> GetAll(Type serviceType) => Provider.GetServices(serviceType);

		public T TryGet<T>() => Provider.GetService<T>();

		public IEnumerable<T> TryGetAll<T>() => Provider.GetService<IEnumerable<T>>();

		public object TryGet(Type serviceType) => Provider.GetService(serviceType);

		public IEnumerable<object> TryGetAll(Type serviceType) => (IEnumerable<object>)Provider.GetService(typeof(IEnumerable<>).MakeGenericType(serviceType));

		public IDependencyResolver CreateChildResolver() => new BaseDependencyResolver(Provider.CreateScope());

		protected void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				_scope?.Dispose();
			}
		}

		protected void InitializeModules()
		{
			if (_moduleInitializers != null)
			{
				foreach (var initializer in _moduleInitializers)
				{
					initializer?.Invoke(this);
				}
			}
		}
	}
}
