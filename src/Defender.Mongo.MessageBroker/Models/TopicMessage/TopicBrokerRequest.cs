using Defender.Mongo.MessageBroker.Models.Base;

namespace Defender.Mongo.MessageBroker.Models
{
    internal class TopicBrokerRequest : BaseBrokerRequest
    {
        public TopicBrokerRequest(string? topicName)
        {
            base.Name = topicName;
        }

        public string? TopicName => base.Name;
    }
}
