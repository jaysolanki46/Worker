using Serilog;
using Worker.Services.Database;
using Worker.Settings;

namespace Worker.Services.EventResource;

/* EventResource - implements the method to generate URI for API request */
public class EventResource: IEventResource
{
    private readonly IDatabaseService _databaseService;
    private readonly ScanEventSettings _scanEventSettings;

    public EventResource(IDatabaseService databaseService, ScanEventSettings scanEventSettings)
    {
        _databaseService = databaseService;
        _scanEventSettings = scanEventSettings;
    }

    /* Generate URI for parcel scan events API */
    public async Task<string> GenerateUriAsync()
    {
        try
        {
            var eventId = await _databaseService.GetLastEventId();

            if (eventId < 1)
                return _scanEventSettings.Uri;
            else
                return $"{_scanEventSettings.Uri}?FromEventId={eventId + 1}&Limit={_scanEventSettings.DefaultLimit}"; /* `eventId + 1` indicating the starting event Id for next request */
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{this.GetType().Name}.{nameof(GenerateUriAsync)} Failed to generate scan event URI");
            throw;
        }
    }
}
