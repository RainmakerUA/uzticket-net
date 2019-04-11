
namespace RM.Lib.Hosting.Contracts.Environment
{
	public sealed class ConfigModule
	{
        public ConfigModule(string assembly)
        {
            Assembly = assembly;
        }

		public string Assembly { get; }
	}
}