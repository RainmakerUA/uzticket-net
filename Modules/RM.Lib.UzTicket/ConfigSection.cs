using System.Collections.Generic;
using RM.Lib.Hosting.Contracts.Config;

namespace RM.Lib.UzTicket
{
    //[ConfigurationSection("seatSorting")]
    public class ConfigSection : IConfigurationSection
    {
        [ConfigurationProperty("useSelf")]
        public bool UseSelf { get; set; }

        [ConfigurationProperty("timeout")]
        public uint DefaultTimeout { get; set; }

        [ConfigurationProperty("module", "assemblyName")]
        public string[] AdditionalModules { get; set; }
    }
}
