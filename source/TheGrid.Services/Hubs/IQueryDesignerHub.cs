using SignalRSwaggerGen.Attributes;

namespace TheGrid.Services.Hubs
{
    [SignalRHub]
    public interface IQueryDesignerHub
    {
        public Task QueryResultsFinishedProcessing(long queryRefreshJobId, int queryId);

        public Task VisualizationOptionsUpdated(int queryId);
    }
}
