using Defender.Mongo.MessageBroker.Models;
using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace Defender.Mongo.MessageBroker.Interfaces.Topic;

public interface ITopicConsumer
{
    ITopicConsumer SetTopic(string topicName);
    ITopicConsumer SetMessageType(string messageType);
    ITopicConsumer SetMessageType(MessageType messageType);

    Task SubscribeTopicAsync<T>(
        Func<T, Task> action,
        Func<DateTime>? startDateProvider,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();

    Task SubscribeTopicAsync<T>(
        Func<T, Task> action,
        Func<Task<DateTime>>? startDateProvider,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();

    Task SubscribeTopicAsync<T>(
        Action<T> action,
        Func<DateTime>? startDateProvider,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();

    Task SubscribeTopicAsync<T>(
        Action<T> action,
        Func<Task<DateTime>>? startDateProvider,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();
}
