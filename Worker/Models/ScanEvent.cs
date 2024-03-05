namespace Worker.Models;

/*
    ScanEvent model - handle single scan event information involved in parcel event response 
*/
public class ScanEvent
{
    public long EventId { get; set; }
    public long ParcelId { get; set; }
    public string Type { get; set; }
    public DateTime CreatedDateTimeUtc { get; set; }
    public DateTime? PickedUpDateTimeUtc { get; set; }
    public DateTime? DeliveredDateTimeUtc { get; set; }
    public string? StatusCode { get; set; }
    public Device? Device { get; set; }
    public User User { get; set; }
}
