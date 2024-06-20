using Defender.Mongo.MessageBroker.Configuration;
using Defender.Mongo.MessageBroker.Configuration.Subscribe;
using Defender.Mongo.MessageBroker.Extensions;
using Defender.Mongo.MessageBroker.Models.Base;
using Defender.Mongo.MessageBroker.Models.QueueMessage;
using Defender.Mongo.MessageBroker.Models.TopicMessage;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Defender.Mongo.MessageBroker.DB;

internal class MongoMessageBroker<T>
    where T : IBaseMessage, new()
{
    private readonly MessageBrokerOptions<T> _brokerOptions;
    private readonly MongoClient _client;
    private readonly IMongoDatabase _database;
    private IMongoCollection<T> _collection;

    private static bool IsTopic => typeof(ITopicMessage).IsAssignableFrom(typeof(T));
    private static bool IsQueue => typeof(IQueueMessage).IsAssignableFrom(typeof(T));

    private readonly SemaphoreSlim _collectionSemaphore = new(1, 1);
    private readonly SemaphoreSlim _processSemaphore = new(10, 10);

    public MongoMessageBroker(MessageBrokerOptions<T> brokerOptions)
    {
        _brokerOptions = brokerOptions;

        ThrowIfNotValidOptions();

        _client = new MongoClient(_brokerOptions.MongoDbConnectionString);
        _database = _client.GetDatabase(_brokerOptions.MongoDbDatabaseName);
        _collection = SetCollectionSafelyAsync().GetAwaiter().GetResult();

        ThrowIfCannotConnectDB();
    }

    public async Task<T> PublishEvent(
        T eventModel,
        CancellationToken cancellationToken = default)
    {
        PrepareModelForInserting(eventModel);

        await _collection.InsertOneAsync(eventModel, null, cancellationToken);

        return eventModel;
    }

    public async Task CreateCursorForCollection(
        SubscribeOptions<T> subscribeOptions,
        CursorType cursorType = CursorType.TailableAwait,
        CancellationToken cancellationToken = default)
    {
        var options = new FindOptions<T>
        {
            CursorType = cursorType
        };

        var fromDate = await subscribeOptions.StartDateProvider();

        // Only for IQueueMessage
        var processId = Guid.NewGuid();

        while (!cancellationToken.IsCancellationRequested)
        {
            var customFilter = await subscribeOptions.FilterProvider();
            var filter = BuildFilter(fromDate);
            filter = customFilter != null ? filter & customFilter : filter;

            try
            {
                using var cursor = await _collection
                    .FindAsync(filter, options, cancellationToken);

                await cursor.ForEachAsync(async document =>
                {
                    if (IsTopic)
                        fromDate = document.InsertedDateTime;

                    await ProcessDocumentAsync(
                        document, subscribeOptions.Action, processId, cancellationToken);

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

    private async Task ProcessDocumentAsync(
        T document,
        Func<T, Task<bool>> action,
        Guid? processId = null,
        CancellationToken cancellationToken = default)
    {
        if (IsTopic)
        {
            try
            {
                await action(document);
            }
            finally { }
            return;
        }

        if (IsQueue)
        {
            var filter = BuildFilter() & Builders<T>.Filter.Eq(x => x.Id, document.Id);

            var update = Builders<T>.Update
                .Set(x => ((IQueueMessage)x).Processing, true)
                .Set(x => ((IQueueMessage)x).ProcessId, processId);

            var options = new FindOneAndUpdateOptions<T> { ReturnDocument = ReturnDocument.After };

            var isSuccess = false;
            await _processSemaphore.WaitAsync(cancellationToken);
            try
            {
                document = await _collection
                    .FindOneAndUpdateAsync(filter, update, options, cancellationToken);

                if (document is null
                    || ((IQueueMessage)document).ProceedDateTime != DateTime.MinValue
                    || ((IQueueMessage)document).ProcessId != processId)
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
                _collection.UpdateOneAsync(
                    Builders<T>.Filter.Eq(x => x.Id, document.Id),
                    Builders<T>.Update
                        .Set(x => ((IQueueMessage)x).ProceedDateTime, DateTime.UtcNow)
                        .Set(x => ((IQueueMessage)x).Processing, false),
                    cancellationToken: cancellationToken).Forget();
            else
                _collection.UpdateOneAsync(
                    Builders<T>.Filter.Eq(x => x.Id, document.Id),
                    Builders<T>.Update.Set(x => ((IQueueMessage)x).Processing, false),
                    cancellationToken: cancellationToken).Forget();
        }
    }

    private FilterDefinition<T> BuildFilter(DateTime? fromDate = null)
    {
        var filterBuilder = new FilterDefinitionBuilder<T>();

        fromDate ??= DateTime.MinValue;

        var filter = filterBuilder.Gte(x => x.InsertedDateTime, fromDate);

        if (!string.IsNullOrWhiteSpace(_brokerOptions.Type))
        {
            filter &= filterBuilder.Eq(x => x.Type, _brokerOptions.Type);
        }

        if (IsQueue)
        {
            var filterQ = filterBuilder.And(
                filterBuilder.Eq(x => ((IQueueMessage)x).ProceedDateTime, DateTime.MinValue),
                filterBuilder.Eq(x => ((IQueueMessage)x).Processing, false)
            );

            filter &= filterQ;
        }

        return filter;
    }



    #region Collections

    private async Task<IMongoCollection<T>> SetCollectionSafelyAsync()
    {
        await _collectionSemaphore.WaitAsync();
        try
        {
            _collection = await GetOrCreateCollectionAsync();

            if (await _collection.EstimatedDocumentCountAsync() == 0)
            {
                await CreateIndexesAsync();
            }
        }
        finally
        {
            _collectionSemaphore.Release();
        }

        return _collection;
    }

    private async Task<IMongoCollection<T>> GetOrCreateCollectionAsync()
    {
        await CreateCollectionIfNotExistsAsync();

        return _database.GetCollection<T>(GetCollectionName);
    }

    private async Task CreateCollectionIfNotExistsAsync()
    {
        if (!await DoesCollectionExistsAsync())
        {
            var createCollectionOptions = new CreateCollectionOptions
            {
                Capped = true,
                MaxDocuments = _brokerOptions.MaxDocuments,
                MaxSize = _brokerOptions.MaxByteSize
            };

            await _database.CreateCollectionAsync(GetCollectionName, createCollectionOptions);
        }
    }

    private async Task CreateIndexesAsync()
    {
        var index = Builders<T>.IndexKeys.Ascending(f => f.InsertedDateTime);
        var indexOptions = new CreateIndexOptions { Background = true };
        await _collection.Indexes.CreateOneAsync(
            new CreateIndexModel<T>(index, indexOptions));

        if (IsQueue)
        {
            var proccIndex = Builders<T>.IndexKeys.Ascending(f => ((IQueueMessage)f).ProceedDateTime);
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<T>(proccIndex, indexOptions));
        }
    }


    private async Task<bool> DoesCollectionExistsAsync()
    {
        var filter = new BsonDocument("name", GetCollectionName);
        var collections = await _database.ListCollectionsAsync(
            new ListCollectionsOptions { Filter = filter });
        return await collections.AnyAsync();
    }


    private string GetCollectionName =>
        IsTopic
            ? GenerateCollectionNameByTopicName()
            : GenerateCollectionNameByQueueName();

    private string GenerateCollectionNameByTopicName() => $"topic-{_brokerOptions.Name}";
    private string GenerateCollectionNameByQueueName() => $"queue-{_brokerOptions.Name}";


    #endregion collections


    private void ThrowIfNotValidOptions()
    {
        if (_brokerOptions is null
            || string.IsNullOrWhiteSpace(_brokerOptions.Name)
            || string.IsNullOrWhiteSpace(_brokerOptions.Type))
            throw new ArgumentException("Invalid options");

        if (IsTopic && IsQueue || !IsTopic && !IsQueue)
            throw new ArgumentException("Event must be either a queue or a topic");
    }

    private void ThrowIfCannotConnectDB()
    {
        if (_client is null || _database is null || _collection is null)
            throw new ArgumentException("MongoDB connection is not initialized");
    }

    private void PrepareModelForInserting(T model)
    {
        model ??= new T();
        model.Type = _brokerOptions.Type;
        model.InsertedDateTime = DateTime.UtcNow;
    }

    private static bool ShouldKeepTrying(Exception ex)
    {
        return ex is MongoConnectionException
            || ex is MongoConnectionPoolPausedException
            || ex is TimeoutException
            || ex is MongoCommandException exc && exc.Code == 136;
    }

}