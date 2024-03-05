using Worker.Services.Database;

namespace Worker.Settings;

public class ScanEventSettings
{
    public string? Uri { set; get; }

    public async Task<string> GenerateUriAsync(IDatabaseService _databaseService)
    {
        var eventId = await _databaseService.GetLastEventId();

        if (eventId < 1)
            return Uri;
        else
            return $"{Uri}?FromEventId={eventId + 1}&Limit=100";
    }
}
