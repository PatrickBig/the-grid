namespace TheGrid.Client.HubClients
{
    public interface IQueryDesignerHubClient
    {
        public void OnQueryResultsFinishedProcessing(Func<long, int, Task> action);

        public void OnVisualizationOptionsUpdated(Func<int, Task> action);
    }
}
