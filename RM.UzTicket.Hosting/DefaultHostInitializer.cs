using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RM.UzTicket.Contracts.ServiceContracts;
using RM.UzTicket.Hosting.Environment;
using RM.UzTicket.Utility;

namespace RM.UzTicket.Hosting
{
	public class DefaultHostInitializer : IHostInitializer
	{
		private Stream _configStream;

		private ILog _logger => LogFactory.GetLog();

		public DefaultHostInitializer()
		{
		}

		public DefaultHostInitializer(Stream configStream)
		{
			_configStream = configStream ?? throw new ArgumentNullException(nameof(configStream));
		}

		public void Initialize(IHostEnvironment environment)
		{
			var reader = new HostConfigReader(_logger);

			if (_configStream == null)
			{
				reader.Read();
			}
			else
			{
				using (_configStream)
				{
					reader.Read(_configStream);
				}

				_configStream = null;
			}

			InitializeCoreType(environment);
			InitializeModules(environment, reader.Modules);
			InitializeSections(environment, reader.Sections);
			InitializeOptions(environment);
		}

		private void InitializeCoreType(IHostEnvironment environment)
		{
			//TODO:?<dependency to="Matrix42.Common.ILog, Matrix42.Common" lifetime="Single" construction="Factory" constructionFactory="Matrix42.Common.LogFactory, Matrix42.Common" constructionMethod="GetLogger" />
			environment.Container.RegisterSingletonType<ILog>(type => LogFactory.GetLog());
		}

		private void InitializeModules(IHostEnvironment environment, IEnumerable<ConfigModule> modules)
		{
			_logger.Info("AppHost: initialize modules");
			var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var module in modules)
			{
				_logger.Info("AppHost: module {0}", module.Assembly);

				var moduleAssembly = domainAssemblies.SingleOrDefault(_ => _.GetName().Name.Equals(module.Assembly, StringComparison.Ordinal));
				Type registratorType;

				try
				{
					if (moduleAssembly == null)
					{
						moduleAssembly = Assembly.Load(module.Assembly);
					}

					registratorType = moduleAssembly.GetTypes().FirstOrDefault(_ => _.GetInterfaces().Any(i => i == typeof (IDependencyRegistrator)));

					if (registratorType == null)
					{
						_logger.Info("AppHost: module {0} without Registrator type.", module.Assembly);
						continue;
					}
				}
				catch (ReflectionTypeLoadException typeLoadException)
				{
					_logger.Error("AppHost: failed while loading assembly {0} types.",  module.Assembly);
					foreach (var loaderEx in typeLoadException.LoaderExceptions)
					{
						_logger.Error("AppHost: TypeLoadError: {0}", loaderEx, loaderEx.Message);
					}

					throw;
				}
				catch (Exception ex)
				{
					_logger.Error("AppHost: module {0} failed to load.", ex, module.Assembly);
					throw;
				}

				try
				{
					var registrator = (IDependencyRegistrator)Activator.CreateInstance(registratorType);
					registrator.Register(environment.Container, environment.Resolver);
				}
				catch (Exception e)
				{
					_logger.Error("Error while executing module initializer '{0}'", e, registratorType.AssemblyQualifiedName);
				}
			}
		}

		public virtual void InitializeSections(IHostEnvironment environment, IEnumerable<ConfigSection> sections)
		{
		}

		public virtual void InitializeOptions(IHostEnvironment environment)
		{
		}
	}
}