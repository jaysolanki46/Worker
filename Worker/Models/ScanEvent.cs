namespace Worker.Models;

public class ScanEvent
{
    public long EventId { get; set; }
    public long ParcelId { get; set; }
    public string Type { get; set; }
    public DateTime CreatedDateTimeUtc { get; set; }
    public string StatusCode { get; set; }
    public Device Device { get; set; }
    public User User { get; set; }
}
