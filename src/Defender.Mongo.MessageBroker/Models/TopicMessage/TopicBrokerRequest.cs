using Defender.Mongo.MessageBroker.Models.Base;

namespace Defender.Mongo.MessageBroker.Models
{
    internal class TopicBrokerRequest : BaseBrokerRequest
    {
        public TopicBrokerRequest(string? topicName)
        {
            TopicName = topicName;
        }

        public string? TopicName { get; set; }
    }
}
