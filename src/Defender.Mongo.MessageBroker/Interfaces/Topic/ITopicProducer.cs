using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace Defender.Mongo.MessageBroker.Interfaces.Topic;

public interface ITopicProducer<T> where T : ITopicMessage, new()
{
    Task<T> PublishTopicAsync(
        T model,
        CancellationToken cancellationToken = default);
}
