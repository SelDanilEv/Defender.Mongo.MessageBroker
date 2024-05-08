using Defender.Mongo.MessageBroker.Models;
using Defender.Mongo.MessageBroker.Models.QueueMessage;

namespace Defender.Mongo.MessageBroker.Interfaces.Queue;

public interface IQueueConsumer
{
    IQueueConsumer SetQueue(string queueName);
    IQueueConsumer SetMessageType(string messageType);
    IQueueConsumer SetMessageType(MessageType messageType);

    Task SubscribeQueueAsync<T>(
        Func<T, Task<bool>> action,
        CancellationToken cancellationToken = default)
        where T : IQueueMessage, new();

    Task SubscribeQueueAsync<T>(
         Func<T, bool> action,
         CancellationToken cancellationToken = default)
             where T : IQueueMessage, new();

    Task CheckQueueAsync<T>(
        Func<T, Task<bool>> action,
        CancellationToken cancellationToken = default)
            where T : IQueueMessage, new();

    Task CheckQueueAsync<T>(
        Func<T, bool> action,
        CancellationToken cancellationToken = default)
            where T : IQueueMessage, new();
}
