using Serilog;
using Worker.Models;
using Worker.Services.Database;

namespace Worker.Settings;

public class ScanEventSettings
{
    public string Uri { set; get; }

    public string DefaultLimit { set; get; }


    /* Generate URI for parcel scan events API */
    public async Task<string> GenerateUriAsync(IDatabaseService _databaseService)
    {
        try
        {
            var eventId = await _databaseService.GetLastEventId();

            if (eventId < 1)
                return Uri;
            else
                return $"{Uri}?FromEventId={eventId + 1}&Limit={DefaultLimit}"; /* `eventId + 1` indicating the starting event Id for next request */
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{this.GetType().Name}.{nameof(GenerateUriAsync)} Failed to generate scan event URI");
            throw;
        }
    }
}
