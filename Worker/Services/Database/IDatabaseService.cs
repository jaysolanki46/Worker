using Worker.Models;

namespace Worker.Services.Database;

public interface IDatabaseService
{
    Task SaveEvent(ScanEvent scanEvent);
    Task SaveLastEvent(LastProcessedScanEvent lastEvent);
    Task<long> GetLastEventId();
}
