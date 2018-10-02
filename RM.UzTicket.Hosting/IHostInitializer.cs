using System.Collections.Generic;
using RM.UzTicket.Contracts.ServiceContracts;
using RM.UzTicket.Hosting.Environment;

namespace RM.UzTicket.Hosting
{
	public interface IHostInitializer
	{
		void Initialize(IHostEnvironment environment);

		void InitializeSections(IHostEnvironment environment, IEnumerable<ConfigSection> sections);

		void InitializeOptions(IHostEnvironment environment);
	}
}