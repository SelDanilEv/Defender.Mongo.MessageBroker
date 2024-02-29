using Defender.Mongo.MessageBroker.Models.TopicMessage;
using MongoDB.Bson.Serialization.Attributes;

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

    [BsonId]
    public Guid Id { get; set; }

    public string Text { get; set; }

    public DateTime ProcessedDateTime
    {
        get; set;
    }
}
