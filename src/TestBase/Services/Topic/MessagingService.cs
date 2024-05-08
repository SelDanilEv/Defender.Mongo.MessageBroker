using Defender.Mongo.MessageBroker.Interfaces;
using Defender.Mongo.MessageBroker.Interfaces.Topic;
using Defender.Mongo.MessageBroker.Models;
using Defender.Mongo.MessageBroker.Models.TopicMessage;
using TestBase.Model.Topic;

namespace TestBase.Services.Topic;

public class MessagingService
{
    private readonly ITopicProducer _producer;

    public MessagingService(ITopicProducer producer)
    {
        _producer = producer;

        _producer.SetTopic(Topics.TextTopic).SetMessageType(MessageType.ClassName);
    }

    public async Task PublishTextAsync(string text)
    {
        var message = new TextMessage(text);

        await _producer.PublishTopicAsync(message);
    }
}
