using Defender.Mongo.MessageBroker.Models;
using Defender.Mongo.MessageBroker.Models.QueueMessage;
using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace Defender.Mongo.MessageBroker.Interfaces.Private;

internal interface IMessageBroker
{
    Task<T> PublishTopicAsync<T>(
        TopicBrokerRequest request,
        T model,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();

    Task SubscribeTopicAsync<T>(
        TopicBrokerRequest request,
        Func<T, Task> action,
        Func<Task<DateTime>>? fromDateTime = null,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();


    Task<T> PublishQueueAsync<T>(
        QueueBrokerRequest request,
        T model,
        CancellationToken cancellationToken = default)
            where T : IQueueMessage, new();

    Task SubscribeQueueAsync<T>(
        QueueBrokerRequest request,
        Func<T, Task<bool>> action,
        CancellationToken cancellationToken = default)
        where T : IQueueMessage, new();

    Task CheckQueueAsync<T>(
        QueueBrokerRequest request,
        Func<T, Task<bool>> action,
        CancellationToken cancellationToken = default)
        where T : IQueueMessage, new();
}
