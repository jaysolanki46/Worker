namespace Worker.Models;

/*
    ScanEventResponse model - handle parcel event response 
*/
public class ScanEventResponse
{
    public IEnumerable<ScanEvent> ScanEvents { get; set; }
}
