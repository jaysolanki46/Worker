using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Worker.Constants;
using Worker.Models;
using Worker.Settings;

namespace Worker.Services.Database;

public class DatabaseService : IDatabaseService
{
    private readonly IMongoDatabase _database;

    public DatabaseService(DatabaseConnectionSettings databaseSettings)
    {
        var client = new MongoClient(databaseSettings.ConnectionString);
        _database = client.GetDatabase(databaseSettings.Database);
    }

    public async Task SaveEvent(ScanEvent scanEvent)
    {
        var collection = _database.GetCollection<ScanEvent>(TableConstants.TABLE_PARCEL_EVENTS);
        await collection.InsertOneAsync(scanEvent);
    }

    public async Task SaveLastEvent(LastProcessedScanEvent lastEvent)
    {
        var collection = _database.GetCollection<LastProcessedScanEvent>(TableConstants.TABLE_PARCEL_LAST_EVENT);
        var filter = Builders<LastProcessedScanEvent>.Filter.Eq(x => x.ParcelId, lastEvent.ParcelId);
        var update = Builders<LastProcessedScanEvent>.Update.Set(x => x.LastEventId, lastEvent.LastEventId);

        await collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    public async Task<long> GetLastEventId()
    {
        var collection = _database.GetCollection<LastProcessedScanEvent>(TableConstants.TABLE_PARCEL_LAST_EVENT);
        var projection = Builders<LastProcessedScanEvent>.Projection.Include(x => x.LastEventId);

        var bsonDocument = await collection.Find(Builders<LastProcessedScanEvent>.Filter.Empty)
                                          .Project(projection)
                                          .FirstOrDefaultAsync();

        if (bsonDocument != null)
        {
            var lastProcessedScanEvent = BsonSerializer.Deserialize<LastProcessedScanEvent>(bsonDocument);
            return lastProcessedScanEvent?.LastEventId ?? 0;
        }

        return 0;
    }
}
