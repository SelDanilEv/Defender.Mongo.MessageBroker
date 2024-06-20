using Defender.Mongo.MessageBroker.Configuration.Subscribe;
using Defender.Mongo.MessageBroker.Models.QueueMessage;

namespace Defender.Mongo.MessageBroker.Interfaces.Queue;

public interface IQueueConsumer<T> where T : IQueueMessage, new()
{
    Task SubscribeQueueAsync(
        SubscribeOptions<T> options,
        CancellationToken cancellationToken = default);

    Task RetryMissedEventsAsync(
        SubscribeOptions<T> options,
        CancellationToken cancellationToken = default);
}
