using Defender.Mongo.MessageBroker.Interfaces.Private;
using Defender.Mongo.MessageBroker.Interfaces.Queue;
using Defender.Mongo.MessageBroker.Models;
using Defender.Mongo.MessageBroker.Models.QueueMessage;

namespace Defender.Mongo.MessageBroker.Processing;

internal class QueueMessageProcessor(IMessageBroker messageBroker) : IQueueConsumer, IQueueProducer
{
    private readonly IMessageBroker _messageBroker = messageBroker;

    private QueueBrokerRequestBuilder RequestBuilder { get; set; } = new QueueBrokerRequestBuilder();

    #region core 
    public async Task<T> PublishQueueAsync<T>(
        T model,
        CancellationToken cancellationToken = default)
            where T : IQueueMessage, new()
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        return await _messageBroker.PublishQueueAsync(
            RequestBuilder.Build<T>(),
            model,
            cancellationToken);
    }

    public async Task SubscribeQueueAsync<T>(
        Func<T, Task<bool>> action,
        CancellationToken cancellationToken = default)
        where T : IQueueMessage, new()
    {
        await _messageBroker.SubscribeQueueAsync(
            RequestBuilder.Build<T>(),
            action,
            cancellationToken);
    }

    public async Task SubscribeQueueAsync<T>(
        Func<T,bool> action,
        CancellationToken cancellationToken = default)
            where T : IQueueMessage, new()
    {
        await _messageBroker.SubscribeQueueAsync(
            RequestBuilder.Build<T>(),
            ConvertToAsyncFunc(action),
            cancellationToken);
    }


    public async Task CheckQueueAsync<T>(
        Func<T, Task<bool>> action,
        CancellationToken cancellationToken = default)
        where T : IQueueMessage, new()
    {
        await _messageBroker.CheckQueueAsync(
            RequestBuilder.Build<T>(),
            action,
            cancellationToken);
    }

    public async Task CheckQueueAsync<T>(
        Func<T, bool> action,
        CancellationToken cancellationToken = default)
            where T : IQueueMessage, new()
    {
        await _messageBroker.CheckQueueAsync(
            RequestBuilder.Build<T>(),
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
        RequestBuilder.SetQueue(queueName);
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
        RequestBuilder.SetMessageType(messageType);
        return this;
    }

    private QueueMessageProcessor SetMessageType(MessageType messageType)
    {
        RequestBuilder.SetMessageType(messageType);
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