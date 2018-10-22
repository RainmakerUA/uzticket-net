namespace RM.UzTicket.Settings.Contracts
{
    public interface IUzSettings
    {
        string BaseUrl { get; }
        
        string SessionCookie { get; }
    }
}
