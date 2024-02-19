using Defender.Mongo.MessageBroker.Models.ProcessedEvent;
using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace TestBase.Model;

public class TextMessage : BaseTopicMessage, IProcessedEvent
{
    public TextMessage()
    {
    }

    public TextMessage(string text)
    {
        Text = text;
    }

    public string Text { get; set; }

    public DateTime ProcessedDateTime
    {
        get; set;
    }
}
