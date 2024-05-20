using Defender.Mongo.MessageBroker.Configuration;
using Defender.Mongo.MessageBroker.Extensions;
using Defender.Mongo.MessageBroker.Interfaces.Private;
using Defender.Mongo.MessageBroker.Models;
using Defender.Mongo.MessageBroker.Models.Base;
using Defender.Mongo.MessageBroker.Models.QueueMessage;
using Defender.Mongo.MessageBroker.Models.TopicMessage;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Defender.Mongo.MessageBroker.Processing;

internal class MongoMessageBroker : IMessageBroker
{
    private readonly MongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly long _maxTopicDocuments;
    private readonly long _maxTopicByteSize;
    private readonly long _maxQueueDocuments;
    private readonly long _maxQueueByteSize;
    private readonly SemaphoreSlim _collectionSemaphore = new(1, 1);
    private readonly SemaphoreSlim _processSemaphore = new(10, 10);

    public MongoMessageBroker(IOptions<MessageBrokerOptions> options)
    {
        _client ??= new MongoClient(options.Value.MongoDbConnectionString);
        _database ??= _client.GetDatabase(options.Value.MongoDbDatabaseName);

        _maxTopicDocuments = options.Value.MaxTopicDocuments;
        _maxTopicByteSize = options.Value.MaxTopicByteSize;
        _maxQueueDocuments = options.Value.MaxQueueDocuments;
        _maxQueueByteSize = options.Value.MaxQueueByteSize;
    }

    #region queue

    public async Task<T> PublishQueueAsync<T>(
        QueueBrokerRequest request,
        T model,
        CancellationToken cancellationToken = default)
            where T : IQueueMessage, new()
    {
        if (request == null
            || request.QueueName == null
            || request.MessageType == null) throw new ArgumentNullException(nameof(request));

        var _mongoCollection = await GetOrCreateQueueCollectionAsync<T>(request);

        PrepareModel(model, request.MessageType);

        await _mongoCollection.InsertOneAsync(model, null, cancellationToken);

        return model;
    }

    public async Task SubscribeQueueAsync<T>(
        QueueBrokerRequest request,
        Func<T, Task<bool>> action,
        CancellationToken cancellationToken = default)
        where T : IQueueMessage, new()
    {
        if (request == null
            || request.QueueName == null
            || request.MessageType == null) throw new ArgumentNullException(nameof(request));

        var _mongoCollection = await GetOrCreateQueueCollectionAsync<T>(request);

        var options = new FindOptions<T>
        {
            CursorType = CursorType.TailableAwait
        };

        var filter = BuildFilter<T>(request);

        var processId = Guid.NewGuid();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var cursor = await _mongoCollection
                    .FindAsync(filter, options, cancellationToken);

                await cursor.ForEachAsync(async document =>
                {
                    await ProcessDocument(
                        _mongoCollection,
                        document,
                        action,
                        processId,
                        cancellationToken);
                }, cancellationToken: cancellationToken);
            }
            catch (Exception ex) when (ShouldKeepTrying(ex))
            {
                //just keep trying reconnect
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public async Task CheckQueueAsync<T>(
        QueueBrokerRequest request,
        Func<T, Task<bool>> action,
        CancellationToken cancellationToken = default)
        where T : IQueueMessage, new()
    {
        if (request == null
            || request.QueueName == null
            || request.MessageType == null) throw new ArgumentNullException(nameof(request));

        var currentDateTime = DateTime.UtcNow;

        var _mongoCollection = await GetOrCreateQueueCollectionAsync<T>(request);

        var options = new FindOptions<T>
        {
            CursorType = CursorType.NonTailable
        };

        var filter = BuildFilter<T>(request);

        filter = Builders<T>.Filter.And(filter,
            Builders<T>.Filter.Lte(x => x.InsertedDateTime, currentDateTime));

        var processId = Guid.NewGuid();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var cursor = await _mongoCollection
                    .FindAsync(filter, options, cancellationToken);

                await cursor.ForEachAsync(document =>
                {
                    ProcessDocument(
                        _mongoCollection,
                        document,
                        action,
                        processId,
                        cancellationToken).Forget();
                }, cancellationToken: cancellationToken);

                break;
            }
            catch (Exception ex) when (ShouldKeepTrying(ex))
            {
                //just keep trying reconnect
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    private static FilterDefinition<T> BuildFilter<T>(
            QueueBrokerRequest request)
        where T : IQueueMessage, new()
    {
        var filterBuilder = Builders<T>.Filter;
        var filter = filterBuilder.And(
            filterBuilder.Gt(x => x.InsertedDateTime, DateTime.MinValue.AddTicks(1)),
            filterBuilder.Eq(x => x.ProceedDateTime, DateTime.MinValue),
            filterBuilder.Eq(x => x.Processing, false)
        );

        if (request.MessageType != string.Empty)
        {
            filter = filterBuilder.And(
                filter,
                filterBuilder.Eq(x => x.Type, request.MessageType));
        }

        return filter;
    }

    private async Task ProcessDocument<T>(
        IMongoCollection<T> mongoCollection,
        T document,
        Func<T, Task<bool>> action,
        Guid processId,
        CancellationToken cancellationToken) where T : IQueueMessage, new()
    {
        var filter = Builders<T>.Filter.And(
            Builders<T>.Filter.Eq(x => x.Id, document.Id),
            Builders<T>.Filter.Eq(x => x.Processing, false),
            Builders<T>.Filter.Eq(x => x.ProceedDateTime, DateTime.MinValue)
            );
        var update = Builders<T>.Update
            .Set(x => x.Processing, true)
            .Set(x => x.ProcessId, processId);
        var options = new FindOneAndUpdateOptions<T> { ReturnDocument = ReturnDocument.After };

        var isSuccess = false;
        await _processSemaphore.WaitAsync(cancellationToken);
        try
        {
            document = await mongoCollection
                .FindOneAndUpdateAsync(filter, update, options, cancellationToken);

            if (document == null
                || document.ProceedDateTime != DateTime.MinValue
                || document.ProcessId != processId)
            {
                // Another service is already processing this document
                return;
            }

            isSuccess = await action(document);
        }
        finally
        {
            _processSemaphore.Release();
        }


        if (isSuccess)
            mongoCollection.UpdateOneAsync(
                Builders<T>.Filter.Eq(x => x.Id, document.Id),
                Builders<T>.Update
                    .Set(x => x.ProceedDateTime, DateTime.UtcNow)
                    .Set(x => x.Processing, false),
                cancellationToken: cancellationToken).Forget();
        else
            mongoCollection.UpdateOneAsync(
                Builders<T>.Filter.Eq(x => x.Id, document.Id),
                Builders<T>.Update.Set(x => x.Processing, false),
                cancellationToken: cancellationToken).Forget();
    }

    #endregion

    #region topic

    public async Task<T> PublishTopicAsync<T>(
        TopicBrokerRequest request,
        T model,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new()
    {
        if (request == null
            || request.TopicName == null
            || request.MessageType == null) throw new ArgumentNullException(nameof(request));

        var _mongoCollection = await GetOrCreateTopicCollectionAsync<T>(request);

        PrepareModel(model, request.MessageType);

        await _mongoCollection.InsertOneAsync(model, null, cancellationToken);

        return model;
    }

    public async Task SubscribeTopicAsync<T>(
        TopicBrokerRequest request,
        Func<T, Task> action,
        Func<Task<DateTime>>? fromDateTime = null,
        CancellationToken cancellationToken = default)
        where T : ITopicMessage, new()
    {
        if (request == null
            || request.TopicName == null)
            throw new ArgumentNullException(nameof(request));

        var _mongoCollection = await GetOrCreateTopicCollectionAsync<T>(request);

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
            var filter = filterBuilder.Gte(x => x.InsertedDateTime, lastInsertDate);

            if (request.MessageType != string.Empty)
            {
                filter = filterBuilder.And(
                    filter,
                    filterBuilder.Eq(x => x.Type, request.MessageType));
            }

            try
            {
                using var cursor = await _mongoCollection.FindAsync(
                    filter, options, cancellationToken);

                await cursor.ForEachAsync(async document =>
                {
                    lastInsertDate = document.InsertedDateTime;
                    await action(document);
                });
            }
            catch (Exception ex) when (ShouldKeepTrying(ex))
            {
                //just keep trying reconnect
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    #endregion topic

    #region Collections

    public async Task EnsureTopicCollectionExistsAsync(BaseBrokerRequest request)
    {
        var name = request.GetName;
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var collectionName = GetCollectionNameByTopicName(name);

        await CreateCollectionIfNotExists(
            collectionName,
            request?.MaxDocuments ?? _maxTopicDocuments,
            request?.MaxByteSize ?? _maxTopicByteSize);
    }

    public async Task EnsureQueueCollectionExistsAsync(BaseBrokerRequest request)
    {
        var name = request.GetName;
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var collectionName = GetCollectionNameByQueueName(name);

        await CreateCollectionIfNotExists(
            collectionName,
            request?.MaxDocuments ?? _maxTopicDocuments,
            request?.MaxByteSize ?? _maxTopicByteSize);
    }

    private async Task<IMongoCollection<T>> GetOrCreateTopicCollectionAsync<T>(
        TopicBrokerRequest request)
        where T : ITopicMessage, new()
    {
        request.SetName(GetCollectionNameByTopicName(request.TopicName));

        return await GetOrCreateCollectionAsync<T>(request);
    }

    private async Task<IMongoCollection<T>> GetOrCreateQueueCollectionAsync<T>(
        QueueBrokerRequest request)
        where T : IQueueMessage, new()
    {
        request.SetName(GetCollectionNameByQueueName(request.QueueName));

        return await GetOrCreateCollectionAsync<T>(request);
    }

    private async Task<IMongoCollection<T>> GetOrCreateCollectionAsync<T>(
        BaseBrokerRequest request)
        where T : IBaseMessage, new()
    {
        var mongoCollection = await GetOrCreateCollection<T>(request);

        await _collectionSemaphore.WaitAsync();
        try
        {
            if (await mongoCollection.EstimatedDocumentCountAsync() == 0)
            {
                await CreateIndexes(mongoCollection);
            }
        }
        finally
        {
            _collectionSemaphore.Release();
        }

        return mongoCollection;
    }

    private async Task<IMongoCollection<T>> GetOrCreateCollection<T>(
        BaseBrokerRequest request)
    {
        await CreateCollectionIfNotExists(
            request.GetName,
            request.MaxDocuments ?? (typeof(T) == typeof(ITopicMessage)
                ? _maxTopicDocuments
                : _maxQueueDocuments),
            request.MaxByteSize ?? (typeof(T) == typeof(ITopicMessage)
                ? _maxTopicByteSize
                : _maxQueueByteSize));

        return _database.GetCollection<T>(request.GetName);
    }

    private async Task CreateCollectionIfNotExists(
        string collectionName,
        long maxDocuments,
        long maxByteSize)
    {
        if (!await CollectionExistsAsync(collectionName))
        {
            var createCollectionOptions = new CreateCollectionOptions
            {
                Capped = true,
                MaxDocuments = maxDocuments,
                MaxSize = maxByteSize
            };

            await _database.CreateCollectionAsync(collectionName, createCollectionOptions);
        }
    }

    private static async Task CreateIndexes<T>(IMongoCollection<T> mongoCollection)
        where T : IBaseMessage, new()
    {
        var index = Builders<T>.IndexKeys.Ascending(f => f.InsertedDateTime);
        var indexOptions = new CreateIndexOptions { Background = true };
        await mongoCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<T>(index, indexOptions));

        if (mongoCollection is IMongoCollection<IQueueMessage> queueMessageCollection)
        {
            await CreateQueueMessageIndex(queueMessageCollection);
        }
    }

    private static async Task CreateQueueMessageIndex<T>(IMongoCollection<T> mongoCollection)
        where T : IQueueMessage
    {
        var proccIndex = Builders<T>.IndexKeys.Ascending(f => f.ProceedDateTime);
        var indexOptions = new CreateIndexOptions { Background = true };
        await mongoCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<T>(proccIndex, indexOptions));
    }

    private async Task<bool> CollectionExistsAsync(string collectionName)
    {
        var filter = new BsonDocument("name", collectionName);
        var collections = await _database.ListCollectionsAsync(
            new ListCollectionsOptions { Filter = filter });
        return await collections.AnyAsync();
    }

    private static string GetCollectionNameByTopicName(string topic) => $"topic-{topic}";
    private static string GetCollectionNameByQueueName(string topic) => $"queue-{topic}";

    #endregion collections

    private static void PrepareModel<T>(T model, string messageType)
        where T : IBaseMessage, new()
    {
        model ??= new T();
        model.InsertedDateTime = DateTime.UtcNow;
        model.Type = messageType;
    }

    private static bool ShouldKeepTrying(Exception ex)
    {
        return ex is MongoConnectionException
            || ex is MongoConnectionPoolPausedException
            || ex is TimeoutException;
    }

}