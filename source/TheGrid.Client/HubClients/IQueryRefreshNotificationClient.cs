namespace TheGrid.Client.HubClients
{
    public interface IQueryRefreshNotificationClient
    {
        public void OnQueryResultsFinishedProcessing(Func<long, int, Task> action);
    }
}
