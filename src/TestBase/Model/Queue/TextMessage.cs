using Defender.Mongo.MessageBroker.Models.QueueMessage;

namespace TestBase.Model.Queue;

public class TextMessage : BaseQueueMessage
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
