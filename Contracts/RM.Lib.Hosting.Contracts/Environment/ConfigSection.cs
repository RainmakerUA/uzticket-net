
namespace RM.Lib.Hosting.Contracts.Environment
{
    public sealed class ConfigSection
    {
        public ConfigSection(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }

        public string Type { get; }
    }
}