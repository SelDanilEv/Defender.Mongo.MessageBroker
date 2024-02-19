using Defender.Mongo.MessageBroker.Configuration;
using Defender.Mongo.MessageBroker.Models.ProcessedEvent;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TestBase.Repositories
{
    public class TestRepository<T> where T : IProcessedEvent
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<T> _collection;

        public TestRepository(IOptions<MessageBrokerOptions> options)
        {
            _client ??= new MongoClient(options.Value.MongoDbConnectionString);
            _database ??= _client.GetDatabase(options.Value.MongoDbDatabaseName);

            _collection = _database.GetCollection<T>("test-collection");
        }

        public async Task<T> Insert(T model)
        {
            await _collection.InsertOneAsync(model);

            return model;
        }

        public async Task<List<T>> GetAll()
        {
            return await _collection
                .Find(new BsonDocument())
                .ToListAsync();
        }

        public async Task<T> GetLast()
        {
            var filter = Builders<T>.Filter.Empty;
            var sort = Builders<T>.Sort.Descending(x => x.ProcessedDateTime);

            return await _collection.Find(filter).Sort(sort).Limit(1).FirstOrDefaultAsync(); ;
        }
    }
}
