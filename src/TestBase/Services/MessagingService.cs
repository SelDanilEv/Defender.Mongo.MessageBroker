using Defender.Mongo.MessageBroker.Interfaces;
using Defender.Mongo.MessageBroker.Models.TopicMessage;
using TestBase.Model;

namespace TestBase.Services;

public class MessagingService
{
    private readonly IProducer _producer;

    public MessagingService(IProducer producer)
    {
        _producer = producer;

        _producer.SetTopic(Topics.TextTopic).SetMessageType(MessageType.ClassName);
    }

    public async Task PublishTextAsync(string text)
    {
        var message = new TextMessage(text);

        await _producer.PublishAsync(message);
    }
}
