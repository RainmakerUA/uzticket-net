using RM.Lib.Hosting.Contracts.Config;

namespace RM.Lib.UzTicket
{
    [ConfigurationSection("boo")]
    public sealed class BooSection : IConfigurationSection
    {
        public string Test { get; set; }
    }
}
