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
    private readonly SemaphoreSlim _collectionSemaphore = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _processSemaphore = new SemaphoreSlim(10, 10);

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

        var _mongoCollection = await GetQueueCollectionAsync<T>(request.QueueName);

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

        var _mongoCollection = await GetQueueCollectionAsync<T>(request.QueueName);

        var options = new FindOptions<T>
        {
            CursorType = CursorType.TailableAwait
        };

        var filter = BuildFilter<T>(request);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using (var cursor = await _mongoCollection.FindAsync(filter, options, cancellationToken))
                {
                    await cursor.ForEachAsync(async document =>
                    {
                        await ProcessDocument(_mongoCollection, document, filter, action, cancellationToken);
                    });
                }
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

        var _mongoCollection = await GetQueueCollectionAsync<T>(request.QueueName);

        var options = new FindOptions<T>
        {
            CursorType = CursorType.NonTailable
        };


        var filter = BuildFilter<T>(request);

        filter = Builders<T>.Filter.And(filter,
            Builders<T>.Filter.Lte(x => x.InsertedDateTime, currentDateTime));

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using (var cursor = await _mongoCollection.FindAsync(filter, options, cancellationToken))
                {
                    await cursor.ForEachAsync(document =>
                    {
                        ProcessDocument(
                            _mongoCollection,
                            document, 
                            filter,
                            action, 
                            cancellationToken).Forget();
                    });
                }

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

    private FilterDefinition<T> BuildFilter<T>(
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
        FilterDefinition<T> filter,
        Func<T, Task<bool>> action,
        CancellationToken cancellationToken) where T : IQueueMessage, new()
    {
        var update = Builders<T>.Update.Set(x => x.Processing, true);
        var options = new FindOneAndUpdateOptions<T> { ReturnDocument = ReturnDocument.After };

        document = await mongoCollection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);

        if (document == null || document.ProceedDateTime != DateTime.MinValue)
        {
            // Another service is already processing this document
            return;
        }

        var isSuccess = false;
        await _processSemaphore.WaitAsync();
        try
        {
            isSuccess = await action(document);
        }
        finally
        {
            _processSemaphore.Release();
        }


        if (isSuccess)
            mongoCollection.UpdateOneAsync(
                Builders<T>.Filter.Eq(x => x.Id, document.Id),
                Builders<T>.Update.Set(x => x.ProceedDateTime, DateTime.UtcNow)).Forget();
        else
            mongoCollection.UpdateOneAsync(
                Builders<T>.Filter.Eq(x => x.Id, document.Id),
                Builders<T>.Update.Set(x => x.Processing, false)).Forget();
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

        var _mongoCollection = await GetTopicCollectionAsync<T>(request.TopicName);

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

        var _mongoCollection = await GetTopicCollectionAsync<T>(request.TopicName);

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
            catch (Exception)
            {
                throw;
            }
        }
    }

    #endregion topic

    #region Collections
    private async Task<IMongoCollection<T>> GetTopicCollectionAsync<T>(string topic) where T : IBaseMessage, new()
    {
        var collectionName = GetCollectionNameByTopicName(topic);

        return await GetCollectionAsync<T>(collectionName);
    }

    private async Task<IMongoCollection<T>> GetQueueCollectionAsync<T>(string queue) where T : IQueueMessage, new()
    {
        var collectionName = GetCollectionNameByQueueName(queue);

        return await GetCollectionAsync<T>(collectionName);
    }


    private async Task<IMongoCollection<T>> GetCollectionAsync<T>(string collectionName) where T : IBaseMessage, new()
    {
        var mongoCollection = await GetCollection<T>(collectionName);

        await _collectionSemaphore.WaitAsync();
        try
        {
            if (await mongoCollection.EstimatedDocumentCountAsync() == 0)
            {
                await CreateInitRecord(mongoCollection);
            }
        }
        finally
        {
            _collectionSemaphore.Release();
        }

        return mongoCollection;
    }

    private async Task<IMongoCollection<T>> GetCollection<T>(string collectionName)
    {
        if (!await CollectionExistsAsync(collectionName))
        {
            var createCollectionOptions = new CreateCollectionOptions();
            createCollectionOptions.Capped = true;
            createCollectionOptions.MaxDocuments = typeof(T) == typeof(ITopicMessage) ? _maxTopicDocuments : _maxQueueDocuments;
            createCollectionOptions.MaxSize = typeof(T) == typeof(ITopicMessage) ? _maxTopicByteSize : _maxQueueByteSize;

            await _database.CreateCollectionAsync(collectionName, createCollectionOptions);
        }

        return _database.GetCollection<T>(collectionName);
    }


    private async Task CreateInitRecord<T>(IMongoCollection<T> mongoCollection) where T : IBaseMessage, new()
    {
        var initMessage = new T();
        initMessage.InsertedDateTime = DateTime.MinValue;
        await mongoCollection.InsertOneAsync(initMessage);

        var index = Builders<T>.IndexKeys.Ascending(f => f.InsertedDateTime);
        var indexOptions = new CreateIndexOptions { Background = true };
        await mongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<T>(index, indexOptions));

        if (mongoCollection is IMongoCollection<IQueueMessage> queueMessageCollection)
        {
            await CreateQueueMessageIndex(queueMessageCollection);
        }
    }

    private async Task CreateQueueMessageIndex<T>(IMongoCollection<T> mongoCollection) where T : IQueueMessage
    {
        var procc = Builders<T>.IndexKeys.Ascending(f => f.ProceedDateTime);
        var indexOptions2 = new CreateIndexOptions { Background = true };
        await mongoCollection.Indexes.CreateOneAsync(new CreateIndexModel<T>(procc, indexOptions2));
    }

    private async Task<bool> CollectionExistsAsync(string collectionName)
    {
        var filter = new BsonDocument("name", collectionName);
        var collections = await _database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
        return await collections.AnyAsync();
    }

    private string GetCollectionNameByTopicName(string topic) => $"topic-{topic}";
    private string GetCollectionNameByQueueName(string topic) => $"queue-{topic}";
    #endregion collections

    private void PrepareModel<T>(T model, string messageType) where T : IBaseMessage, new()
    {
        if (model == null) model = new T();
        model.InsertedDateTime = DateTime.UtcNow;
        model.Type = messageType;
    }

    private bool ShouldKeepTrying(Exception ex)
    {
        return ex is MongoConnectionException
            || ex is MongoConnectionPoolPausedException
            || ex is TimeoutException;
    }

}