using Defender.Mongo.MessageBroker.Models.Base;

namespace Defender.Mongo.MessageBroker.Models
{
    internal class QueueBrokerRequest : BaseBrokerRequest
    {
        public QueueBrokerRequest(string? queueName)
        {
            QueueName = queueName;
        }

        public string? QueueName { get; set; }
    }
}
