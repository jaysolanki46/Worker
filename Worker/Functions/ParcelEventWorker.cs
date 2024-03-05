using Newtonsoft.Json;
using Worker.Models;
using Worker.Services.Database;
using Worker.Settings;

namespace Worker.Functions;

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
        string jsonString = @"
        {
            ""ScanEvents"": [
                {
                    ""EventId"": 83269,
                    ""ParcelId"": 5002,
                    ""Type"": ""PICKUP"",
                    ""CreatedDateTimeUtc"": ""2021-05-11T21:11:34.1506147Z"",
                    ""StatusCode"": """",
                    ""Device"": {
                        ""DeviceTransactionId"": 83269,
                        ""DeviceId"": 103
                    },
                    ""User"": {
                        ""UserId"": ""NC1001"",
                        ""CarrierId"": ""NC"",
                        ""RunId"": ""100""
                    }
                }
            ]
        }";

        long lastEventId = 0;
        long lastParcelId = 0;
        while (!token.IsCancellationRequested)
        {
            try
            {
                var uri = await _scanEventSettings.GenerateUriAsync(_databaseService);
                var response = await _httpClient.GetStringAsync(uri);
                var scanEventResponse = JsonConvert.DeserializeObject<ScanEventResponse>(jsonString);

                if (scanEventResponse != null && scanEventResponse.ScanEvents.Any())
                {
                    lastEventId = scanEventResponse.ScanEvents.Last().EventId;
                    lastParcelId = scanEventResponse.ScanEvents.Last().ParcelId;

                    foreach (var scanEvent in scanEventResponse.ScanEvents)
                    {
                        await _databaseService.SaveEvent(scanEvent);
                    }

                }
                else
                {
                    Console.WriteLine("API response does not contain scan events.");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error making API request: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(10), token);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing API response: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
            finally
            {
                if (lastEventId >= 1) await _databaseService.SaveLastEvent(new LastProcessedScanEvent() { LastEventId = lastEventId, ParcelId = lastParcelId });
            }
        }
    }

}
