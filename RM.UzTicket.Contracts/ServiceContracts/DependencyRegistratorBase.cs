using System;
using System.Reflection;

namespace RM.UzTicket.Contracts.ServiceContracts
{
	public abstract class DependencyRegistratorBase
	{
		private const string _defaultConfigResourceName = "Properties.module.config";

		protected static void LoadModuleConfig(IDependencyContainer container, string configPath = null)
		{
			var assembly = Assembly.GetCallingAssembly();

			configPath = configPath ?? _defaultConfigResourceName;
			configPath = String.Format("{0}.{1}", assembly.GetName().Name, configPath);
			
			using (var resourceStream = assembly.GetManifestResourceStream(configPath))
			{
				if (resourceStream != null)
				{
					container.LoadModuleConfig(resourceStream);
				}
			}
		}

		protected static void RegisterModuleInitializer(IDependencyResolver resolver, Action<IDependencyResolver> initializer)
		{
			resolver.RegisterModuleInitializer(initializer);
		}
	}
}