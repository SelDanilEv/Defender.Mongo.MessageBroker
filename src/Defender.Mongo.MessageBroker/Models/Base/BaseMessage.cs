using MongoDB.Bson.Serialization.Attributes;

namespace Defender.Mongo.MessageBroker.Models.Base
{
    public abstract class BaseMessage : IBaseMessage
    {
        [BsonId]
        public Guid Id { get; set; }
        public DateTime InsertedDateTime { get; set; }
        public string? Type { get; set; }
    }
}
