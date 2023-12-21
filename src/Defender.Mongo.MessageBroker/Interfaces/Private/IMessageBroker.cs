using Defender.Mongo.MessageBroker.Models;
using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace Defender.Mongo.MessageBroker.Interfaces.Private;

internal interface IMessageBroker
{
    Task PublsihAsync<T>(
        BrokerRequest request,
        T model,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();

    Task SubscribeAsync<T>(
        BrokerRequest request,
        Func<T, Task> action,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();

    Task SubscribeAsync<T>(
        BrokerRequest request,
        Action<T> action,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();
}
