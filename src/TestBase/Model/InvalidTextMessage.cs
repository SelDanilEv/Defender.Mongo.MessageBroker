using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace TestBase.Model;

public class InvalidTextMessage : BaseTopicMessage
{
    public InvalidTextMessage()
    {
    }

    public InvalidTextMessage(string text)
    {
        Text1 = text;
    }

    public string Text1 { get; set; }
}
