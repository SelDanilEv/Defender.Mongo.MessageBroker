using Defender.Mongo.MessageBroker.Interfaces.Private;
using Defender.Mongo.MessageBroker.Interfaces.Topic;
using Defender.Mongo.MessageBroker.Models;
using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace Defender.Mongo.MessageBroker.Processing;

internal class TopicMessageProcessor : ITopicConsumer, ITopicProducer
{
    private readonly IMessageBroker _messageBroker;

    private TopicBrokerRequestBuilder _requestBuilder { get; set; }

    public TopicMessageProcessor(IMessageBroker messageBroker)
    {
        _messageBroker = messageBroker;
        _requestBuilder = new TopicBrokerRequestBuilder();
    }

    #region core 
    public async Task<T> PublishTopicAsync<T>(
        T model,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new()
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        return await _messageBroker.PublishTopicAsync(
            _requestBuilder.Build<T>(),
            model,
            cancellationToken);
    }

    public async Task SubscribeTopicAsync<T>(
    Func<T, Task> action,
    Func<Task<DateTime>>? fromDateTime,
    CancellationToken cancellationToken = default)
        where T : ITopicMessage, new()
    {
        await _messageBroker.SubscribeTopicAsync(
            _requestBuilder.Build<T>(),
            action,
            fromDateTime,
            cancellationToken);
    }

    public async Task SubscribeTopicAsync<T>(
        Func<T, Task> action,
        Func<DateTime>? startDateProvider,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new()
    {
        await _messageBroker.SubscribeTopicAsync(
            _requestBuilder.Build<T>(),
            action,
            MapStartDateProvider(startDateProvider),
            cancellationToken);
    }

    public async Task SubscribeTopicAsync<T>(
        Action<T> action,
        Func<DateTime>? startDateProvider,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new()
    {
        await _messageBroker.SubscribeTopicAsync(
            _requestBuilder.Build<T>(),
            ConvertToAsyncFunc(action),
            MapStartDateProvider(startDateProvider),
            cancellationToken);
    }

    public async Task SubscribeTopicAsync<T>(
        Action<T> action,
        Func<Task<DateTime>>? fromDateTime,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new()
    {
        await _messageBroker.SubscribeTopicAsync(
            _requestBuilder.Build<T>(),
            ConvertToAsyncFunc(action),
            fromDateTime,
            cancellationToken);
    }
    #endregion

    #region Topic 
    ITopicProducer ITopicProducer.SetTopic(string topicName)
    {
        return SetTopic(topicName);
    }

    ITopicConsumer ITopicConsumer.SetTopic(string topicName)
    {
        return SetTopic(topicName);
    }

    private TopicMessageProcessor SetTopic(string topicName)
    {
        _requestBuilder.SetTopic(topicName);
        return this;
    }
    #endregion

    #region Message type
    ITopicConsumer ITopicConsumer.SetMessageType(string messageType)
    {
        return SetMessageType(messageType);
    }

    ITopicConsumer ITopicConsumer.SetMessageType(MessageType messageType)
    {
        return SetMessageType(messageType);
    }

    ITopicProducer ITopicProducer.SetMessageType(string messageType)
    {
        return SetMessageType(messageType);
    }

    ITopicProducer ITopicProducer.SetMessageType(MessageType messageType)
    {
        return SetMessageType(messageType);
    }

    private TopicMessageProcessor SetMessageType(string messageType)
    {
        _requestBuilder.SetMessageType(messageType);
        return this;
    }

    private TopicMessageProcessor SetMessageType(MessageType messageType)
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