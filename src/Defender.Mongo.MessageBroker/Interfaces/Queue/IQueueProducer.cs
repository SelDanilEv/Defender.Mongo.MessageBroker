using Defender.Mongo.MessageBroker.Models;
using Defender.Mongo.MessageBroker.Models.QueueMessage;

namespace Defender.Mongo.MessageBroker.Interfaces.Queue;

public interface IQueueProducer
{
    IQueueProducer SetQueue(string queueName);
    IQueueProducer SetMessageType(string messageType);
    IQueueProducer SetMessageType(MessageType messageType);

    Task<T> PublishQueueAsync<T>(
        T model,
        CancellationToken cancellationToken = default)
            where T : IQueueMessage, new();
}
