namespace Defender.Mongo.MessageBroker.Models.Base
{
    internal abstract class BaseBrokerRequest
    {
        public string? MessageType { get; set; }
    }
}
