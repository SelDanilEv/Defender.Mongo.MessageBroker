using Defender.Mongo.MessageBroker.Models;
using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace Defender.Mongo.MessageBroker.Interfaces.Topic;

public interface ITopicProducer : ICollectionManager
{
    ITopicProducer SetTopic(string topicName);
    ITopicProducer SetMessageType(string messageType);
    ITopicProducer SetMessageType(MessageType messageType);
    ITopicProducer SetMaxDocuments(long maxDocuments);
    ITopicProducer SetMaxByteSize(long maxByteSize);

    Task<T> PublishTopicAsync<T>(
        T model,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();
}
