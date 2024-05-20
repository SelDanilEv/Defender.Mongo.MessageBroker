using Defender.Mongo.MessageBroker.Configuration;
using Defender.Mongo.MessageBroker.Models.Base;
using Defender.Mongo.MessageBroker.Models.TopicMessage;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TestBase.Repositories
{
    public class TestRepository<T> where T : IBaseMessage
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<T> _collection;

        public TestRepository(IOptions<MessageBrokerOptions> options)
        {
            _client ??= new MongoClient(options.Value.MongoDbConnectionString);
            _database ??= _client.GetDatabase(options.Value.MongoDbDatabaseName);

            var sufix = typeof(T) == typeof(Model.Topic.TextMessage) ? "2" : "1";

            _collection = _database.GetCollection<T>("test-collection-" + sufix);
        }

        public async Task<T> Insert(T model)
        {
            try
            {
                await _collection.InsertOneAsync(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

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
            return await _collection
                .Find(new BsonDocument()).FirstOrDefaultAsync();
        }
    }
}
