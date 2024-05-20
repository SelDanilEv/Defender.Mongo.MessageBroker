using Defender.Mongo.MessageBroker.Models.Base;

namespace Defender.Mongo.MessageBroker.Models.TopicMessage
{
    public abstract record BaseTopicMessage : BaseMessage, ITopicMessage
    {
    }
}
