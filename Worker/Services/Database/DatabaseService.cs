using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Serilog;
using Worker.Constants;
using Worker.Models;
using Worker.Settings;

namespace Worker.Services.Database;

/* DatabaseService - implementation of IDatabaseService, includes the method bodies to save/update event, save/update last event, fetch the last event */
public class DatabaseService : IDatabaseService
{
    private readonly IMongoDatabase _database;

    /* Initiate database connection */
    public DatabaseService(DatabaseConnectionSettings databaseSettings)
    {
        try
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            _database = client.GetDatabase(databaseSettings.Database);

            Log.Information($"Successfully connected to database");
            Log.Debug($"{this.GetType().Name}.{nameof(DatabaseService)} Successfully connected to database: {databaseSettings.ConnectionString}, Database: {databaseSettings.Database}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{this.GetType().Name}.{nameof(DatabaseService)} Failed to connect to database: {ex.Message}");
            throw;
        }
    }

    /* Save or Update most recent event agaist parcel */
    public async Task SaveEvent(ScanEvent scanEvent)
    {
        try
        {
            var collection = _database.GetCollection<ScanEvent>(TableConstants.TABLE_PARCEL_EVENTS);
            var filter = Builders<ScanEvent>.Filter.Eq(x => x.ParcelId, scanEvent.ParcelId);

            var update = Builders<ScanEvent>.Update
                .Set(x => x.EventId, scanEvent.EventId)
                .Set(x => x.Type, scanEvent.Type)
                .Set(x => x.CreatedDateTimeUtc, scanEvent.CreatedDateTimeUtc)
                .Set(x => x.StatusCode, scanEvent.StatusCode)
                .Set(x => x.User.RunId, scanEvent.User.RunId);

            switch (scanEvent.Type?.ToUpper())
            {
                case "PICKUP":
                    update = update.Set(x => x.PickedUpDateTimeUtc, scanEvent.CreatedDateTimeUtc);
                    break;
                case "DELIVERY":
                    update = update.Set(x => x.DeliveredDateTimeUtc, scanEvent.CreatedDateTimeUtc);
                    break;
                default:
                    Log.Warning($"{this.GetType().Name}.{nameof(SaveEvent)} Unsupported parcel event type: {scanEvent.Type}.");
                    break;
            }

            await collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{this.GetType().Name}.{nameof(SaveEvent)} Failed save parcel event. ParcelId: {scanEvent.ParcelId}, EventId: {scanEvent.EventId}, Type: {scanEvent.Type}");
            throw;
        }
    }

    /* Delete old document and save latest last event id */
    public async Task SaveLastEvent(LastProcessedScanEvent lastEvent)
    {
        try
        {
            var collection = _database.GetCollection<LastProcessedScanEvent>(TableConstants.TABLE_PARCEL_LAST_EVENT);
            await collection.DeleteManyAsync(FilterDefinition<LastProcessedScanEvent>.Empty);
            await collection.InsertOneAsync(lastEvent);

            Log.Information($"{this.GetType().Name}.{nameof(SaveLastEvent)} Successfully saved last processed parcel event. ParcelId: {lastEvent.ParcelId}, LastEventId: {lastEvent.LastEventId}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{this.GetType().Name}.{nameof(SaveLastEvent)} Failed to save last processed parcel event. ParcelId: {lastEvent.ParcelId}, LastEventId: {lastEvent.LastEventId}");
            throw;
        }
    }

    /* Get last event id */
    public async Task<long> GetLastEventId()
    {
        try
        {
            var inValidLastEventId = 0;
            var collection = _database.GetCollection<LastProcessedScanEvent>(TableConstants.TABLE_PARCEL_LAST_EVENT);
            var projection = Builders<LastProcessedScanEvent>.Projection.Include(x => x.LastEventId);

            var bsonDocument = await collection.Find(Builders<LastProcessedScanEvent>.Filter.Empty)
                                              .Project(projection)
                                              .FirstOrDefaultAsync();

            if (bsonDocument != null)
            {
                var lastProcessedScanEvent = BsonSerializer.Deserialize<LastProcessedScanEvent>(bsonDocument);
                return lastProcessedScanEvent?.LastEventId ?? inValidLastEventId;
            }

            return inValidLastEventId;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"{this.GetType().Name}.{nameof(GetLastEventId)} Failed to get last processed parcel event.");
            throw;
        }
    }
}
