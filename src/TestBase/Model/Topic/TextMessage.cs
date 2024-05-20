using Defender.Mongo.MessageBroker.Models.TopicMessage;
using MongoDB.Bson.Serialization.Attributes;

namespace TestBase.Model.Topic;

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

    public DateTime Timestamp
    {
        get; set;
    }
}
