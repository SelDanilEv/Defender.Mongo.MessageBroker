using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace TestBase.Model;

public class TextMessage : BaseTopicMessage
{
    public TextMessage()
    {
    }

    public TextMessage(string text)
    {
        Text = text;
    }

    public string Text { get; set; }
}
