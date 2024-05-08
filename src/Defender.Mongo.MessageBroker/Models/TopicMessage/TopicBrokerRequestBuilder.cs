using Defender.Mongo.MessageBroker.Models.Base;

namespace Defender.Mongo.MessageBroker.Models
{
    internal class TopicBrokerRequestBuilder : BaseBrokerRequestBuilder
    {
        public string TopicName { get; set; } = "default";

        public TopicBrokerRequest Build<T>()
        {
            var result = new TopicBrokerRequest(TopicName);

            base.Build<T>(result);

            return result;
        }

        public void SetTopic(string topicName)
        {
            this.TopicName = topicName;
        }
    }
}
