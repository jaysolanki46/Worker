using Moq;
using Worker.Functions;
using Worker.Models;
using Worker.Services.Database;
using Worker.Services.EventResource;
using Worker.Settings;
using Xunit;

namespace Worker.Tests;

public class ScanEventWorkerTests
{
    private readonly Mock<IDatabaseService> _databaseServiceMock;
    private readonly Mock<IEventResource> _eventResource;
    private readonly Mock<ScanEventSettings> _scanEventSettingsMock;
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly ScanEventWorker _scanEventWorker;

    // Arrange the objects
    public ScanEventWorkerTests()
    {
        _databaseServiceMock = new Mock<IDatabaseService>();
        _eventResource = new Mock<IEventResource>();
        _scanEventSettingsMock = new Mock<ScanEventSettings>();
        _httpClientMock = new Mock<HttpClient>();

        _scanEventWorker = new ScanEventWorker(_databaseServiceMock.Object, _eventResource.Object, _scanEventSettingsMock.Object, _httpClientMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulExecution()
    {
        // Arrange the dummy response
        string dummyEvent = @"
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

        _eventResource.Setup(x => x.GenerateUriAsync()).ReturnsAsync("http://test.com");
        _httpClientMock.Setup(x => x.GetStringAsync(It.IsAny<string>())).ReturnsAsync(dummyEvent);
        var tokenSource = new CancellationTokenSource();

        // Act - Execute test case
        await _scanEventWorker.ExecuteAsync(tokenSource.Token);

        // Assert - Verify the results
        _databaseServiceMock.Verify(x => x.SaveEvent(It.IsAny<ScanEvent>()), Times.Once);
        _databaseServiceMock.Verify(x => x.SaveLastEvent(It.IsAny<LastProcessedScanEvent>()), Times.Once);
    }
}