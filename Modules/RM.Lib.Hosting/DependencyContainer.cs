using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RM.Lib.Common.Contracts.Log;
using RM.Lib.Hosting.Contracts;
using RM.Lib.Hosting.Contracts.Environment;
using RM.Lib.Hosting.Environment;
using RM.Lib.Utility;

namespace RM.Lib.Hosting
{
	internal sealed class DependencyContainer : BaseDependencyResolver, IDependencyContainer
	{
		private readonly IServiceCollection _services;
		
		private ILog _log => LogFactory.GetLog();

		public DependencyContainer()
		{
			_services = new ServiceCollection();

			_services.AddSingleton<IDependencyResolver>(this);
			_services.AddSingleton<IDependencyContainer>(this);
		}

		public void InitializeProvider()
		{
			Provider = _services.BuildServiceProvider(true);
			InitializeModules();
		}

		#region IDependencyContainer

		public void RegisterType<TInterface, TClass>() where TInterface : class where TClass : class, TInterface
		{
			_services.AddTransient<TInterface, TClass>();

			_log.Debug("RegisterType<{0}, {1}>()", typeof(TInterface), typeof(TClass));
		}

		public void RegisterType<TInterface>(Func<Type, object> factoryFunc) where TInterface : class
		{
			_services.AddTransient(sp => (TInterface)factoryFunc(typeof(TInterface)));

			_log.Debug("RegisterType<{0}>(Func<Type, object> factoryFunc = {1})", typeof(TInterface), factoryFunc);
		}

		public void RegisterType(Type interfaceType, Type classType)
		{
			_services.AddTransient(interfaceType, classType);

			_log.Debug("RegisterType(Type interfaceType = {0}, classType = {1})", interfaceType, classType);
		}

		public void RegisterType(Type interfaceType, Func<Type, object> factoryFunc)
		{
			_services.AddTransient(interfaceType, sp => factoryFunc(interfaceType));

			_log.Debug("RegisterType(interfaceType = {0}, Func<Type, object> factoryFunc = {1})", interfaceType, factoryFunc);
		}

		// TODO: Register scoped

		public void RegisterSingletonType<TInterface, TClass>() where TInterface : class where TClass : class, TInterface
		{
			_services.AddSingleton<TInterface, TClass>();

			_log.Debug("RegisterSingletonType<{0}, {1}>()", typeof(TInterface), typeof(TClass));
		}

		public void RegisterSingletonType<TInterface>(Func<Type, TInterface> factoryFunc) where TInterface : class
		{
			_services.AddSingleton(sp => factoryFunc(typeof(TInterface)));

			_log.Debug("RegisterSingletonType<{0}>(Func<Type, object> factoryFunc = {1})", typeof(TInterface), factoryFunc);
		}

		public void RegisterSingletonType(Type interfaceType, Type classType)
		{
			_services.AddSingleton(interfaceType, classType);

			_log.Debug("RegisterSingletonType(Type interfaceType = {0}, classType = {1})", interfaceType, classType);
		}

		public void RegisterSingletonType(Type interfaceType, Func<Type, object> factoryFunc)
		{
			_services.AddSingleton(interfaceType, sp => factoryFunc(interfaceType));

			_log.Debug("RegisterSingletonType(interfaceType = {0}, Func<Type, object> factoryFunc = {1})", interfaceType, factoryFunc);
		}

		public void RegisterSingletonType(Type interfaceType, Func<object> factoryFunc)
		{
			_services.AddSingleton(interfaceType, sp => factoryFunc());

			_log.Debug("RegisterSingletonType(interfaceType = {0}, Func<object> factoryFunc = {1})", interfaceType, factoryFunc);
		}
		
		public void RegisterInstance<TInterface>(TInterface value) where TInterface : class
		{
			_services.AddSingleton(value);

			_log.Debug("RegisterInstance<{0}>({1} value = {2})", typeof(TInterface), value.GetType(), value);
		}

		public void RegisterInstance(Type interfaceType, object value)
		{
			_services.AddSingleton(interfaceType, value);

			_log.Debug("RegisterInstance(interfaceType = {0}, object value = {1})", interfaceType, value);
		}

		public void LoadModuleConfig(Stream configDataSteam)
		{
			var reader = new ModuleConfigReader();
			reader.Read(configDataSteam);

			InitializeComponents(reader.Dependencies);
		}

		#endregion

		private void InitializeComponents(IEnumerable<ConfigDependency> dependencies)
		{
			foreach (var component in dependencies)
			{
				var toType = Type.GetType(component.To, true);
				Type fromType = null;

				if (component.Construction != Construction.Factory)
				{
					fromType = Type.GetType(component.From, true);
				}

				if (component.Lifetime == Lifetime.Single)
				{
					if (component.Construction == Construction.Mapped)
					{
						RegisterSingletonType(toType, fromType);
					}
					else if (component.Construction == Construction.Factory)
					{
						var factoryType = Type.GetType(component.ConstructionFactory, true);
						var factoryMethod = factoryType.GetMethod(component.ConstructionMethod, BindingFlags.Public | BindingFlags.Static);
						var parametersCount = factoryMethod.GetParameters().Length;
						var genericArgsCount = factoryMethod.GetGenericArguments().Length;

						if (parametersCount > 0 || genericArgsCount > 0)
						{
							var lambda = Expression.Lambda<Func<Type, object>>(Expression.Call(factoryType, component.ConstructionMethod, null, Expression.Parameter(typeof(Type))));
							var func = lambda.Compile();
							RegisterSingletonType(toType, func);
						}
						else
						{
							var lambda = Expression.Lambda<Func<object>>(Expression.Call(factoryMethod));
							var func = lambda.Compile();
							RegisterSingletonType(toType, func);
						}
					}
				}
				else if (component.Lifetime == Lifetime.PerCall)
				{
					if (component.Construction == Construction.Mapped)
					{
						RegisterType(toType, fromType);
					}
					else if (component.Construction == Construction.Factory)
					{
						var factoryType = Type.GetType(component.ConstructionFactory, true);
						var paramExpr = Expression.Parameter(typeof(Type), "p");
						var expr = Expression.Call(factoryType, component.ConstructionMethod, null, paramExpr);
						var lambda = Expression.Lambda<Func<Type, object>>(expr, paramExpr);
						var func = lambda.Compile();

						RegisterType(toType, func);
					}
				}
			}
		}
	}
}