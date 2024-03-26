using Defender.Mongo.MessageBroker.Configuration;
using Defender.Mongo.MessageBroker.Interfaces.Private;
using Defender.Mongo.MessageBroker.Models;
using Defender.Mongo.MessageBroker.Models.TopicMessage;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Defender.Mongo.MessageBroker.Processing;

internal class MongoMessageBroker : IMessageBroker
{
    private readonly MongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly long _maxDocuments;
    private readonly long _maxByteSize;

    public MongoMessageBroker(IOptions<MessageBrokerOptions> options)
    {
        _client ??= new MongoClient(options.Value.MongoDbConnectionString);
        _database ??= _client.GetDatabase(options.Value.MongoDbDatabaseName);

        _maxDocuments = options.Value.MaxTopicDocuments;
        _maxByteSize = options.Value.MaxTopicByteSize;
    }

    public async Task SubscribeAsync<T>(
        BrokerRequest request,
        Func<T, Task> action,
        Func<Task<DateTime>>? fromDateTime = null,
        CancellationToken cancellationToken = default)
        where T : ITopicMessage, new()
    {
        await SubscribeInternalAsync<T>(
            request,
            async document =>
            {
                await action(document);
            },
            fromDateTime,
            cancellationToken);
    }

    public async Task PublsihAsync<T>(
        BrokerRequest request,
        T model,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new()
    {
        var _mongoCollection = await GetCollectionAsync<T>(request.TopicName);

        PrepareModel(model, request.MessageType);

        await _mongoCollection.InsertOneAsync(model, null, cancellationToken);
    }

    private async Task SubscribeInternalAsync<T>(
    BrokerRequest request,
    Func<T, Task> action,
    Func<Task<DateTime>>? fromDateTime,
    CancellationToken cancellationToken)
    where T : ITopicMessage, new()
    {
        var _mongoCollection = await GetCollectionAsync<T>(request.TopicName);

        var options = new FindOptions<T>
        {
            CursorType = CursorType.TailableAwait
        };

        var lastInsertDate = DateTime.UtcNow;

        if (fromDateTime != null)
            lastInsertDate = await fromDateTime();

        var filterBuilder = Builders<T>.Filter;

        while (!cancellationToken.IsCancellationRequested)
        {
            var filter = filterBuilder.Gt(x => x.InsertedDateTime, lastInsertDate);

            if (request.MessageType != string.Empty)
            {
                filter = filterBuilder.And(
                    filter, 
                    filterBuilder.Eq(x => x.Type, request.MessageType));
            }

            try
            {
                using (var cursor = await _mongoCollection.FindAsync(filter, options, cancellationToken))
                {
                    await cursor.ForEachAsync(async document =>
                    {
                        lastInsertDate = document.InsertedDateTime;
                        await action(document);
                    });
                }
            }
            catch (Exception ex) when (ShouldKeepTrying(ex))
            {
                //just keep trying reconnect
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

    private async Task<IMongoCollection<T>> GetCollectionAsync<T>(string topic) where T : ITopicMessage, new()
    {
        var collectionName = GetCollectionNameByTopic(topic);

        IMongoCollection<T> mongoCollection;

        if (!await CollectionExistsAsync(collectionName))
        {
            var createCollectionOptions = new CreateCollectionOptions();
            createCollectionOptions.Capped = true;
            createCollectionOptions.MaxDocuments = _maxDocuments;
            createCollectionOptions.MaxSize = _maxByteSize;

            await _database.CreateCollectionAsync(collectionName, createCollectionOptions);

        }

        mongoCollection = _database.GetCollection<T>(collectionName);

        if (await mongoCollection.EstimatedDocumentCountAsync() == 0)
        {
            var initMessage = new T();
            initMessage.InsertedDateTime = DateTime.MinValue;
            await mongoCollection.InsertOneAsync(initMessage);

            var index = Builders<T>.IndexKeys.Ascending(f => f.InsertedDateTime);
            var indexOptions = new CreateIndexOptions { Background = true };
            await mongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<T>(index, indexOptions));
        }

        return mongoCollection;
    }

    private void PrepareModel<T>(T model, string messageType) where T : ITopicMessage, new()
    {
        if (model == null) model = new T();
        model.InsertedDateTime = DateTime.UtcNow;
        model.Type = messageType;
    }

    private async Task<bool> CollectionExistsAsync(string collectionName)
    {
        var filter = new BsonDocument("name", collectionName);
        var collections = await _database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
        return await collections.AnyAsync();
    }

    private string GetCollectionNameByTopic(string topic) => $"topic-{topic}";

    private bool ShouldKeepTrying(Exception ex)
    {
        return ex is MongoConnectionException
            || ex is MongoConnectionPoolPausedException
            || ex is TimeoutException;
    }

}