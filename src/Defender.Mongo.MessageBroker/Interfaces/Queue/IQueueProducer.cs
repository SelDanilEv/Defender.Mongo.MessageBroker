using Defender.Mongo.MessageBroker.Models.QueueMessage;

namespace Defender.Mongo.MessageBroker.Interfaces.Queue;

public interface IQueueProducer<T> where T : IQueueMessage, new()
{
    Task<T> PublishQueueAsync(
        T model,
        CancellationToken cancellationToken = default);
}
