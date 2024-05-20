using Defender.Mongo.MessageBroker.Models.Base;

namespace Defender.Mongo.MessageBroker.Models
{
    internal class QueueBrokerRequest : BaseBrokerRequest
    {
        public QueueBrokerRequest(string? queueName)
        {
            Name = queueName;
        }

        public string? QueueName => base.Name;
    }
}
