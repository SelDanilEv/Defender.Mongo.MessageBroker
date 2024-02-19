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
        Func<Task<DateTime>>? fromDateTime = null,
        CancellationToken cancellationToken = default)
            where T : ITopicMessage, new();
}
