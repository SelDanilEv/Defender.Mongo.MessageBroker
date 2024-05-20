using Defender.Mongo.MessageBroker.Models.Base;

namespace Defender.Mongo.MessageBroker.Models.QueueMessage
{
    public abstract record BaseQueueMessage : BaseMessage, IQueueMessage
    {
        public DateTime ProceedDateTime { get; set; }
        public bool Processing { get; set; }
        public Guid ProcessId { get; set; }
    }
}
