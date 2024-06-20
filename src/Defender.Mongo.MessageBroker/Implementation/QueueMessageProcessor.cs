using Defender.Mongo.MessageBroker.Configuration.Subscribe;
using Defender.Mongo.MessageBroker.DB;
using Defender.Mongo.MessageBroker.Interfaces.Queue;
using Defender.Mongo.MessageBroker.Models.QueueMessage;
using MongoDB.Driver;

namespace Defender.Mongo.MessageBroker.Implementation;

internal class QueueMessageProcessor<T>(MongoMessageBroker<T> mongoMessageBroker)
    : IQueueConsumer<T>, IQueueProducer<T> where T : IQueueMessage, new()
{
    public async Task<T> PublishQueueAsync(
        T model,
        CancellationToken cancellationToken = default)
    {
        return await mongoMessageBroker.PublishEvent(model, cancellationToken);
    }

    public async Task SubscribeQueueAsync(
        SubscribeOptions<T> subscribeOptions,
        CancellationToken cancellationToken = default)
    {
        await mongoMessageBroker.CreateCursorForCollection(
            subscribeOptions,
            cancellationToken: cancellationToken);
    }

    public async Task RetryMissedEventsAsync(
        SubscribeOptions<T> subscribeOptions,
        CancellationToken cancellationToken = default)
    {
        await mongoMessageBroker.CreateCursorForCollection(
            subscribeOptions,
            CursorType.NonTailable,
            cancellationToken: cancellationToken);
    }
}