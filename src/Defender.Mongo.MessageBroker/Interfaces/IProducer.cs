using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace Defender.Mongo.MessageBroker.Interfaces;

public interface IProducer
{
    IProducer SetTopic(string topicName);
    IProducer SetMessageType(string messageType);
    IProducer SetMessageType(MessageType messageType);

    Task PublishAsync<T>(
        T model,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();
}
