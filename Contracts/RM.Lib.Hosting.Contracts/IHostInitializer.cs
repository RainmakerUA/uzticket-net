using System.Collections.Generic;
using RM.Lib.Hosting.Contracts.Environment;

namespace RM.Lib.Hosting.Contracts
{
	public interface IHostInitializer
	{
		void Initialize(IHostEnvironment environment);

		void InitializeSections(IHostEnvironment environment, IEnumerable<ConfigSection> sections);

		void InitializeOptions(IHostEnvironment environment);
	}
}