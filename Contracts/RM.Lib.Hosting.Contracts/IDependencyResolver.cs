using System;
using System.Collections.Generic;

namespace RM.Lib.Hosting.Contracts
{
	public interface IDependencyResolver: IDisposable
	{
		void RegisterModuleInitializer(Action<IDependencyResolver> initializer);

		T Get<T>();
		//T Get<T>(params ParamMap[] parameters);
		IEnumerable<T> GetAll<T>();
		//IEnumerable<T> GetAll<T>(params ParamMap[] parameters);

		//bool IsRegistered<T>();
		T TryGet<T>();
		//T TryGet<T>(params ParamMap[] parameters);
		IEnumerable<T> TryGetAll<T>();
		//IEnumerable<T> TryGetAll<T>(params ParamMap[] parameters);

		object Activate(Type type);
		object Get(Type serviceType);
		//object Get(Type serviceType, params ParamMap[] parameters);
		IEnumerable<object> GetAll(Type serviceType);
		//IEnumerable<object> GetAll(Type serviceType, params ParamMap[] parameters);

		object TryGet(Type serviceType);
		//object TryGet(Type serviceType, params ParamMap[] parameters);
		IEnumerable<object> TryGetAll(Type serviceType);
		//IEnumerable<object> TryGetAll(Type serviceType, params ParamMap[] parameters);

		IDependencyResolver CreateChildResolver();
	}
}