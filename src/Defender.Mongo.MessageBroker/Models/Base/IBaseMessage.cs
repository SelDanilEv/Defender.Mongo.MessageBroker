using MongoDB.Bson.Serialization.Attributes;

namespace Defender.Mongo.MessageBroker.Models.Base
{
    public interface IBaseMessage
    {
        [BsonId]
        public Guid Id { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.DateTime)]
        public DateTime InsertedDateTime { get; set; }
        public string? Type { get; set; }
    }
}
