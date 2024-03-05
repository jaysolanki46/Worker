using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Worker.Models;

public class LastProcessedScanEvent
{
    public ObjectId _id { get; set; }
    public long LastEventId { get; set; }
    public long ParcelId { get; set; }
}
