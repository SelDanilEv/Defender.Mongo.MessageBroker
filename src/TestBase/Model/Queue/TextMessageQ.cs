using Defender.Mongo.MessageBroker.Models.QueueMessage;

namespace TestBase.Model.Queue;

public record TextMessageQ : BaseQueueMessage
{
    public TextMessageQ()
    {
    }

    public TextMessageQ(string text)
    {
        Text = text;
    }

    public string? Text { get; set; }

}
