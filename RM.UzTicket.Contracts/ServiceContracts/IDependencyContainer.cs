using System;
using System.IO;

namespace RM.UzTicket.Contracts.ServiceContracts
{
	public interface IDependencyContainer
	{
		void RegisterType<TInterface, TClass>(/*bool useDefaultCtor = false*/) where TInterface : class where TClass : class, TInterface;
		//void RegisterType<TInterface, TClass>(string entryName/*, bool useDefaultCtor = false*/) where TInterface : class where TClass : class, TInterface;
		void RegisterType<TInterface>(Func<Type, object> factoryFunc) where TInterface : class;

		void RegisterType(Type interfaceType, Type classType/*, bool useDefaultCtor = false*/);
		void RegisterType(Type interfaceType, Func<Type, object> factoryFunc);

		void RegisterSingletonType<TInterface, TClass>(/*bool useDefaultCtor = false*/) where TInterface : class where TClass : class, TInterface;
		void RegisterSingletonType<TInterface>(Func<Type, TInterface> factoryFunc) where TInterface : class;
		void RegisterSingletonType(Type interfaceType, Type classType/*, bool useDefaultCtor = false*/);
		void RegisterSingletonType(Type interfaceType, Func<Type, object> factoryFunc);
		void RegisterSingletonType(Type interfaceType, Func<object> factoryFunc);
		//void RegisterSingletonType<TInterface, TClass>(string name) where TClass : TInterface;
		//void RegisterSingletonType<TInterface>(string name, Func<Type, object> factoryFunc);

		void RegisterInstance(Type interfaceType, object value);
		void RegisterInstance<TInterface>(TInterface value) where TInterface : class;

		void LoadModuleConfig(Stream configDataSteam);
	}
}
