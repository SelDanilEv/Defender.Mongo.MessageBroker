using Defender.Mongo.MessageBroker.Interfaces.Topic;
using TestBase.Model.Topic;

namespace TestBase.Services.Topic;

public class MessagingService
{
    private readonly ITopicProducer<TextMessageT> _producer;

    public MessagingService(ITopicProducer<TextMessageT> producer)
    {
        _producer = producer;
    }

    public async Task PublishTextAsync(string text)
    {
        var message = new TextMessageT(text);

        await _producer.PublishTopicAsync(message);
    }
}
