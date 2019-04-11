using System;
using System.Collections.Generic;
using RM.Lib.Hosting.Contracts.Config;

namespace RM.Lib.Hosting.Contracts
{
    public interface IDependencyResolver : IDisposable
    {
        void RegisterModuleInitializer(Action<IDependencyResolver> initializer);

        void RegisterModuleInitializer<T>(Action<IDependencyResolver, T> initializer) where T : IConfigurationSection;

        void RegisterModuleInitializer<T>(Action<IDependencyResolver, T[]> initializer) where T : IConfigurationSection;
        
        T Get<T>();

        IEnumerable<T> GetAll<T>();
        
        T TryGet<T>();

        IEnumerable<T> TryGetAll<T>();

        object Activate(Type type);

        object Get(Type serviceType);

        IEnumerable<object> GetAll(Type serviceType);

        object TryGet(Type serviceType);

        IEnumerable<object> TryGetAll(Type serviceType);

        IDependencyResolver CreateChildResolver();
    }
}