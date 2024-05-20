using Defender.Mongo.MessageBroker.Interfaces.Private;
using Defender.Mongo.MessageBroker.Interfaces.Queue;
using Defender.Mongo.MessageBroker.Interfaces.Topic;
using Defender.Mongo.MessageBroker.Models;
using Defender.Mongo.MessageBroker.Models.QueueMessage;

namespace Defender.Mongo.MessageBroker.Processing;

internal class QueueMessageProcessor(IMessageBroker messageBroker) 
    : IQueueConsumer, IQueueProducer
{
    private QueueBrokerRequestBuilder _requestBuilder { get; set; } = 
        new QueueBrokerRequestBuilder();

    #region core 

    public async Task EnsureCollectionExistsAsync()
    {
        await messageBroker.EnsureQueueCollectionExistsAsync(_requestBuilder.Build());
    }

    public async Task<T> PublishQueueAsync<T>(
        T model,
        CancellationToken cancellationToken = default)
            where T : IQueueMessage, new()
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        return await messageBroker.PublishQueueAsync(
            _requestBuilder.Build<T>(),
            model,
            cancellationToken);
    }

    public async Task SubscribeQueueAsync<T>(
        Func<T, Task<bool>> action,
        CancellationToken cancellationToken = default)
        where T : IQueueMessage, new()
    {
        await messageBroker.SubscribeQueueAsync(
            _requestBuilder.Build<T>(),
            action,
            cancellationToken);
    }

    public async Task SubscribeQueueAsync<T>(
        Func<T,bool> action,
        CancellationToken cancellationToken = default)
            where T : IQueueMessage, new()
    {
        await messageBroker.SubscribeQueueAsync(
            _requestBuilder.Build<T>(),
            ConvertToAsyncFunc(action),
            cancellationToken);
    }


    public async Task CheckQueueAsync<T>(
        Func<T, Task<bool>> action,
        CancellationToken cancellationToken = default)
        where T : IQueueMessage, new()
    {
        await messageBroker.CheckQueueAsync(
            _requestBuilder.Build<T>(),
            action,
            cancellationToken);
    }

    public async Task CheckQueueAsync<T>(
        Func<T, bool> action,
        CancellationToken cancellationToken = default)
            where T : IQueueMessage, new()
    {
        await messageBroker.CheckQueueAsync(
            _requestBuilder.Build<T>(),
            ConvertToAsyncFunc(action),
            cancellationToken);
    }

    #endregion

    #region Queue 
    IQueueProducer IQueueProducer.SetQueue(string queueName)
    {
        return SetQueue(queueName);
    }

    IQueueConsumer IQueueConsumer.SetQueue(string queueName)
    {
        return SetQueue(queueName);
    }

    private QueueMessageProcessor SetQueue(string queueName)
    {
        _requestBuilder.SetQueue(queueName);
        return this;
    }
    #endregion

    #region Message type
    IQueueConsumer IQueueConsumer.SetMessageType(string messageType)
    {
        return SetMessageType(messageType);
    }

    IQueueConsumer IQueueConsumer.SetMessageType(MessageType messageType)
    {
        return SetMessageType(messageType);
    }

    IQueueProducer IQueueProducer.SetMessageType(string messageType)
    {
        return SetMessageType(messageType);
    }

    IQueueProducer IQueueProducer.SetMessageType(MessageType messageType)
    {
        return SetMessageType(messageType);
    }

    private QueueMessageProcessor SetMessageType(string messageType)
    {
        _requestBuilder.SetMessageType(messageType);
        return this;
    }

    private QueueMessageProcessor SetMessageType(MessageType messageType)
    {
        _requestBuilder.SetMessageType(messageType);
        return this;
    }
    #endregion

    #region Capped collection settings

    IQueueConsumer IQueueConsumer.SetMaxDocuments(long maxDocuments)
    {
        return SetMaxDocuments(maxDocuments);
    }

    IQueueProducer IQueueProducer.SetMaxDocuments(long maxDocuments)
    {
        return SetMaxDocuments(maxDocuments);
    }

    private QueueMessageProcessor SetMaxDocuments(long maxDocuments)
    {
        _requestBuilder.SetMaxDocuments(maxDocuments);
        return this;
    }


    IQueueConsumer IQueueConsumer.SetMaxByteSize(long maxByteSize)
    {
        return SetMaxByteSize(maxByteSize);
    }

    IQueueProducer IQueueProducer.SetMaxByteSize(long maxByteSize)
    {
        return SetMaxByteSize(maxByteSize);
    }

    private QueueMessageProcessor SetMaxByteSize(long maxByteSize)
    {
        _requestBuilder.SetMaxByteSize(maxByteSize);
        return this;
    }

    #endregion

    #region Helpers 

    private static Func<T, Task<bool>> ConvertToAsyncFunc<T>(Func<T, bool> action)
    {
        return (T t) =>
        {
            var result = action(t);
            return Task.FromResult(result);
        };
    }

    #endregion
}