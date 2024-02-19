using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace Defender.Mongo.MessageBroker.Interfaces;

public interface IConsumer
{
    IConsumer SetTopic(string topicName);
    IConsumer SetMessageType(string messageType);
    IConsumer SetMessageType(MessageType messageType);

    Task SubscribeAsync<T>(
        Func<T, Task> action,
        Func<DateTime>? startDateProvider,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();

    Task SubscribeAsync<T>(
        Func<T, Task> action,
        Func<Task<DateTime>>? startDateProvider,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();

    Task SubscribeAsync<T>(
        Action<T> action,
        Func<DateTime>? startDateProvider,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();

    Task SubscribeAsync<T>(
        Action<T> action,
        Func<Task<DateTime>>? startDateProvider,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();
}
