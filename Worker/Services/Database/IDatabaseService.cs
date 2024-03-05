using Worker.Models;

namespace Worker.Services.Database;

/* IDatabaseService interface - includes the methods to save/update event, delete and save last event, fetch the last event */
public interface IDatabaseService
{
    Task SaveEvent(ScanEvent scanEvent);
    Task SaveLastEvent(LastProcessedScanEvent lastEvent);
    Task<long> GetLastEventId();
}
