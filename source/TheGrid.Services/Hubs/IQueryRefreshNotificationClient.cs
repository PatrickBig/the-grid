namespace TheGrid.Services.Hubs
{
    public interface IQueryRefreshNotificationClient
    {
        public Task QueryResultsFinishedProcessing(long queryRefreshJobId, int queryId);
    }
}
