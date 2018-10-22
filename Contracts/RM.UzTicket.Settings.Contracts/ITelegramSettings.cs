namespace RM.UzTicket.Settings.Contracts
{
    public interface ITelegramSettings
    {
        string BotToken { get; }
        
        long? MasterChatID { get; }
    }
}
