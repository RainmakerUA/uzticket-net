using System;

namespace RM.Lib.Hosting.Contracts.Config
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ConfigurationSectionAttribute : Attribute
    {
        public ConfigurationSectionAttribute(string sectionName)
        {
            SectionName = sectionName;
        }

        public string SectionName { get; }
    }
}
