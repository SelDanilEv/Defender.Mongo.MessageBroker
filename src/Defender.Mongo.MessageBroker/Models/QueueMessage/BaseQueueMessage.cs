using Defender.Mongo.MessageBroker.Models.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Defender.Mongo.MessageBroker.Models.QueueMessage
{
    public abstract record BaseQueueMessage : BaseMessage, IQueueMessage
    {
        public DateTime ProceedDateTime { get; set; }
        public bool Processing { get; set; }
        [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
        public Guid ProcessId { get; set; }
    }
}
