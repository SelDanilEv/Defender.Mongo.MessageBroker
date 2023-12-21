using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace TestBase.Model;

public class InvalidTextMessageModel : BaseTopicMessage
{
    public InvalidTextMessageModel()
    {
    }

    public InvalidTextMessageModel(string text)
    {
        InvalidText = text;
    }

    public string InvalidText { get; set; }
}
