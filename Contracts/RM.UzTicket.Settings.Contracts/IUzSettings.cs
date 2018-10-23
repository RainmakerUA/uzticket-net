namespace RM.UzTicket.Settings.Contracts
{
    public interface IUzSettings
    {
        string BaseUrl { get; }
        
        string SessionCookie { get; }

		int? ScanDelay { get; }

		string Temp { get; }
    }
}
