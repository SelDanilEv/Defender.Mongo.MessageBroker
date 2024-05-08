using Defender.Mongo.MessageBroker.Models;
using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace Defender.Mongo.MessageBroker.Interfaces.Topic;

public interface ITopicProducer
{
    ITopicProducer SetTopic(string topicName);
    ITopicProducer SetMessageType(string messageType);
    ITopicProducer SetMessageType(MessageType messageType);

    Task<T> PublishTopicAsync<T>(
        T model,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();
}
