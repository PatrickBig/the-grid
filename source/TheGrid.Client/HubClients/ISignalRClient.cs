namespace TheGrid.Client.HubClients
{
    public interface ISignalRClient
    {
        bool IsConnected { get; }

        Task Start();
    }
}
