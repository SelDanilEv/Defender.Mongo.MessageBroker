namespace Defender.Mongo.MessageBroker.Models.TopicMessage
{
    public abstract class BaseTopicMessage : ITopicMessage
    {
        public DateTime InsertedDateTime { get; set; }
        public string? Type { get; set; }
    }
}
