namespace Defender.Mongo.MessageBroker.Models
{
    internal class BrokerRequest
    {
        public string? TopicName { get; set; }
        public string MessageType { get; set; }
    }
}
