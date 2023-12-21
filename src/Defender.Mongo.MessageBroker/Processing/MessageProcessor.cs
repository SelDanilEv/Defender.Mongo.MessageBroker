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
        await _messageBroker.PublsihAsync(_requestBuilder.Build<T>(), model, cancellationToken);
    }

    public async Task SubscribeAsync<T>(
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new()
    {
        await _messageBroker.SubscribeAsync(_requestBuilder.Build<T>(), action, cancellationToken);
    }

    public async Task SubscribeAsync<T>(
        Action<T> action,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new()
    {
        await _messageBroker.SubscribeAsync(_requestBuilder.Build<T>(), action, cancellationToken);
    }
    #endregion

    #region Set topic 
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

    #region Set message type
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
}