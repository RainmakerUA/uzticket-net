namespace RM.UzTicket.Settings.Contracts
{
    public interface IPersistenceSettings
    {
        string DatabaseUrl { get; }
        
        string DatabasePassword { get; }

		string ConnectionString { get; }
    }
}
