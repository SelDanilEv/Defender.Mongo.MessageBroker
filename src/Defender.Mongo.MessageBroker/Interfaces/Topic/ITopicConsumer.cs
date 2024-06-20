using Defender.Mongo.MessageBroker.Configuration.Subscribe;
using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace Defender.Mongo.MessageBroker.Interfaces.Topic;

public interface ITopicConsumer<T> where T : ITopicMessage, new()
{
    Task SubscribeTopicAsync(
        SubscribeOptions<T> subscribeOptions,
        CancellationToken cancellationToken = default);
}
