using Defender.Mongo.MessageBroker.Models.Base;

namespace Defender.Mongo.MessageBroker.Models.QueueMessage
{
    public abstract class BaseQueueMessage : BaseMessage, IQueueMessage
    {
        public DateTime ProceedDateTime { get; set; }
        public bool Processing { get; set; }
    }
}
