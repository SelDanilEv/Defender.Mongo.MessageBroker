using Defender.Mongo.MessageBroker.Models.QueueMessage;
using Defender.Mongo.MessageBroker.Models.TopicMessage;

namespace TestBase.Model.Queue;

public class InvalidTextMessageModel : BaseQueueMessage
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
