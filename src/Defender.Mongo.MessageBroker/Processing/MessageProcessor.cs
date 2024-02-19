using Defender.Mongo.MessageBroker.Interfaces;
using Defender.Mongo.MessageBroker.Interfaces.Private;
using Defender.Mongo.MessageBroker.Models;
using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace Defender.Mongo.MessageBroker.Processing;

internal class MessageProcessor : IConsumer, IProducer
{
    private readonly IMessageBroker _messageBroker;

    private BrokerRequestBuilder _requestBuilder { get; set; }

    public MessageProcessor(IMessageBroker messageBroker)
    {
        _messageBroker = messageBroker;
        _requestBuilder = new BrokerRequestBuilder();
    }

    #region core 
    public async Task PublishAsync<T>(
        T model,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new()
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        await _messageBroker.PublsihAsync(_requestBuilder.Build<T>(), model, cancellationToken);
    }

    public async Task SubscribeAsync<T>(
    Func<T, Task> action,
    Func<Task<DateTime>>? fromDateTime,
    CancellationToken cancellationToken = default)
        where T : ITopicMessage, new()
    {
        await _messageBroker.SubscribeAsync(
            _requestBuilder.Build<T>(),
            action,
            fromDateTime,
            cancellationToken);
    }

    public async Task SubscribeAsync<T>(
        Func<T, Task> action,
        Func<DateTime>? startDateProvider,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new()
    {
        await _messageBroker.SubscribeAsync(
            _requestBuilder.Build<T>(),
            action,
            MapStartDateProvider(startDateProvider),
            cancellationToken);
    }

    public async Task SubscribeAsync<T>(
        Action<T> action,
        Func<DateTime>? startDateProvider,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new()
    {
        await _messageBroker.SubscribeAsync(
            _requestBuilder.Build<T>(),
            ConvertToAsyncFunc(action),
            MapStartDateProvider(startDateProvider),
            cancellationToken);
    }

    public async Task SubscribeAsync<T>(
        Action<T> action,
        Func<Task<DateTime>>? fromDateTime,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new()
    {
        await _messageBroker.SubscribeAsync(
            _requestBuilder.Build<T>(),
            ConvertToAsyncFunc(action),
            fromDateTime,
            cancellationToken);
    }
    #endregion

    #region Topic 
    IProducer IProducer.SetTopic(string topicName)
    {
        return SetTopic(topicName);
    }

    IConsumer IConsumer.SetTopic(string topicName)
    {
        return SetTopic(topicName);
    }

    private MessageProcessor SetTopic(string topicName)
    {
        _requestBuilder.SetTopic(topicName);
        return this;
    }
    #endregion

    #region Message type
    IConsumer IConsumer.SetMessageType(string messageType)
    {
        return SetMessageType(messageType);
    }

    IConsumer IConsumer.SetMessageType(MessageType messageType)
    {
        return SetMessageType(messageType);
    }

    IProducer IProducer.SetMessageType(string messageType)
    {
        return SetMessageType(messageType);
    }

    IProducer IProducer.SetMessageType(MessageType messageType)
    {
        return SetMessageType(messageType);
    }

    private MessageProcessor SetMessageType(string messageType)
    {
        _requestBuilder.SetMessageType(messageType);
        return this;
    }

    private MessageProcessor SetMessageType(MessageType messageType)
    {
        _requestBuilder.SetMessageType(messageType);
        return this;
    }
    #endregion

    #region Helpers 

    private Func<Task<DateTime>>? MapStartDateProvider(Func<DateTime>? startDateProvider)
    {
        return startDateProvider != null
            ? () => Task.FromResult(startDateProvider())
            : null;
    }

    private Func<T, Task> ConvertToAsyncFunc<T>(Action<T> action)
    {
        return (T t) =>
        {
            action(t);
            return Task.CompletedTask;
        };
    }

    #endregion
}