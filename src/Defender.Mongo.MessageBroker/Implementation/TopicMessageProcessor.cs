using Defender.Mongo.MessageBroker.Configuration;
using Defender.Mongo.MessageBroker.Interfaces.Topic;
using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace Defender.Mongo.MessageBroker.Processing;

internal class TopicMessageProcessor<T>(MongoMessageBroker<T> mongoMessageBroker)
    : ITopicConsumer<T>, ITopicProducer<T> where T : ITopicMessage, new()
{
    public async Task<T> PublishTopicAsync(T model, CancellationToken cancellationToken = default)
    {
        return await mongoMessageBroker.PublishEvent(model, cancellationToken);
    }

    public async Task SubscribeTopicAsync(
        SubscribeOptions<T> subscribeOptions,
        CancellationToken cancellationToken = default)
    {
        await mongoMessageBroker.CreateCursorForCollection(
            subscribeOptions,
            cancellationToken: cancellationToken); ;
    }
}