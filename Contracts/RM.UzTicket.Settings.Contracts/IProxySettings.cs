namespace RM.UzTicket.Settings.Contracts
{
    public interface IProxySettings
    {
        string SourceUrl { get; }
        
        string ScriptPath { get; }
        
        string ProxyPath { get; }
        
        string ProxyRegex { get; }
    }
}
