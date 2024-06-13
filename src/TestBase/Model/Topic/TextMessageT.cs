using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace TestBase.Model.Topic;

public record TextMessageT : BaseTopicMessage
{
    public TextMessageT()
    {
    }

    public TextMessageT(string text)
    {
        Text = text;
    }

    public string? Text { get; set; }

    public DateTime Timestamp
    {
        get; set;
    }
}
