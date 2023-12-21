using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace Defender.Mongo.MessageBroker.Interfaces;

public interface IConsumer
{
    IConsumer SetTopic(string topicName);
    IConsumer SetMessageType(string messageType);
    IConsumer SetMessageType(MessageType messageType);

    Task SubscribeAsync<T>(
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();

    Task SubscribeAsync<T>(
        Action<T> action,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();
}
