using MongoDB.Bson;
namespace Worker.Models;

/*
    LastProcessedScanEvent model - handle last event information involved in parcel event response 
*/
public class LastProcessedScanEvent
{
    public ObjectId _id { get; set; }
    public long LastEventId { get; set; }
    public long ParcelId { get; set; }
}
