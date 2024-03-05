using MongoDB.Libmongocrypt;
using Newtonsoft.Json;
using Serilog;
using Worker.Models;
using Worker.Services.Database;
using Worker.Settings;

namespace Worker.Functions;

/* ParcelEventWorker -  responsible for managing parcel scan events */
public class ParcelEventWorker
{
    private readonly IDatabaseService _databaseService;
    private readonly ScanEventSettings _scanEventSettings;
    private readonly HttpClient _httpClient;

    public ParcelEventWorker(IDatabaseService databaseService, ScanEventSettings scanEventSettings, HttpClient httpClient)
    {
        _databaseService = databaseService;
        _scanEventSettings = scanEventSettings;
        _httpClient = httpClient;
    }

    public async Task ExecuteAsync(CancellationToken token)
    {
        Log.Information($"{this.GetType().Name}.{nameof(ExecuteAsync)} Begin operation");

        long lastEventId = 0;
        long lastParcelId = 0;
        while (!token.IsCancellationRequested)
        {
            try
            {
                // Generate dynamic URI
                var uri = await _scanEventSettings.GenerateUriAsync(_databaseService);
                Log.Debug($"{this.GetType().Name}.{nameof(ExecuteAsync)} Scan event API request URI: {uri}");

                // Fetch the events from external source
                var response = await _httpClient.GetStringAsync(uri);
                Log.Debug($" {this.GetType().Name}.{nameof(ExecuteAsync)}Scan event API response events: {response}");

                // Deserialize response object and map to response class
                var scanEventResponse = JsonConvert.DeserializeObject<ScanEventResponse>(response);

                if (scanEventResponse != null && scanEventResponse.ScanEvents.Any())
                {
                    lastEventId = scanEventResponse.ScanEvents.Last().EventId;
                    lastParcelId = scanEventResponse.ScanEvents.Last().ParcelId;

                    foreach (var scanEvent in scanEventResponse.ScanEvents)
                    {
                        // Save or update most recent scan event against a parcel, and if any event failed to save but continue processing others
                        try
                        {
                            await _databaseService.SaveEvent(scanEvent);
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"{this.GetType().Name}.{nameof(ExecuteAsync)} Failed to save scan event. EventId: {scanEvent.EventId}, ParcelId: {scanEvent.ParcelId}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    Log.Warning("{this.GetType().Name}.{nameof(ExecuteAsync)} Scan event API response is empty.");

                    // 10-second delay to handle empty response and avoid immediate retries.
                    await Task.Delay(TimeSpan.FromSeconds(10), token); 
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Error($"{this.GetType().Name}.{nameof(ExecuteAsync)} Failed to get scan events while http request: {ex.Message}");

                // 10-second delay to handle exceptional cases and avoid immediate retries.
                await Task.Delay(TimeSpan.FromSeconds(10), token);
            }
            catch (JsonException ex)
            {
                Log.Error($"{this.GetType().Name}.{nameof(ExecuteAsync)} Failed to deserialize scan events response: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log.Error($"{this.GetType().Name}.{nameof(ExecuteAsync)} An unexpected error occurredin : {ex.Message}");
            }
            finally
            {
                // Validate last event Id and diffrent from last processed event Id
                if (lastEventId >= 1)
                {
                    var lastProcessedEventId = await _databaseService.GetLastEventId();

                    if (lastProcessedEventId != lastEventId)
                    {
                        // Save last event of the API response
                        await _databaseService.SaveLastEvent(new LastProcessedScanEvent() { LastEventId = lastEventId, ParcelId = lastParcelId });
                        Log.Information($"{this.GetType().Name}.{nameof(ExecuteAsync)} Successfully saved last event: {lastEventId} for parcel scan event");
                    }
                }
            }
        }
    }

}
