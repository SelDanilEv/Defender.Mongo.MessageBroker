using Defender.Mongo.MessageBroker.Models.Base;

namespace Defender.Mongo.MessageBroker.Models
{
    internal class QueueBrokerRequestBuilder : BaseBrokerRequestBuilder
    {
        public string QueueName { get; set; } = "default";

        public QueueBrokerRequest Build<T>()
        {
            var result = new QueueBrokerRequest(QueueName);

            base.Build<T>(result);

            return result;
        }

        public void SetQueue(string queueName)
        {
            this.QueueName = queueName;
        }
    }
}
