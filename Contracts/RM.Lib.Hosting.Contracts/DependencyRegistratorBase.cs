﻿using System;
using System.Reflection;
using RM.Lib.Hosting.Contracts.Config;

namespace RM.Lib.Hosting.Contracts
{
    public abstract class DependencyRegistratorBase
    {
        private const string _defaultConfigResourceName = "Properties.module.config";

        protected static void LoadModuleConfig(IDependencyContainer container, string configPath = null)
        {
            var assembly = Assembly.GetCallingAssembly();

            configPath = configPath ?? _defaultConfigResourceName;
            configPath = $"{assembly.GetName().Name}.{configPath}";

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

        protected static void RegisterModuleInitializer<T>(IDependencyResolver resolver, Action<IDependencyResolver, T> initializer) where T : IConfigurationSection
        {
            resolver.RegisterModuleInitializer(initializer);
        }

        protected static void RegisterModuleInitializer<T>(IDependencyResolver resolver, Action<IDependencyResolver, T[]> initializer) where T : IConfigurationSection
        {
            resolver.RegisterModuleInitializer(initializer);
        }
    }
}