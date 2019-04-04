using System;

namespace RM.Lib.Hosting.Contracts.Config
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ConfigurationPropertyAttribute : Attribute
    {
        public ConfigurationPropertyAttribute(string name, string attributeName = null)
        {
            Name = name;
            AttributeName = attributeName;
        }

        public string Name { get; }

        public string AttributeName { get; }

        public void Deconstruct(out string name, out string attrName)
        {
            name = Name;
            attrName = AttributeName;
        }
    }
}
