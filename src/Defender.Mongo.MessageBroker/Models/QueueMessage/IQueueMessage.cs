using Defender.Mongo.MessageBroker.Models.Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Defender.Mongo.MessageBroker.Models.QueueMessage
{
    public interface IQueueMessage : IBaseMessage
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.DateTime)]
        public DateTime ProceedDateTime { get; set; }
        public bool Processing { get; set; }
    }
}
