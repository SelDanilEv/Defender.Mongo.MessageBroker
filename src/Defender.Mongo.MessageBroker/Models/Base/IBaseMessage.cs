using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Defender.Mongo.MessageBroker.Models.Base
{
    public interface IBaseMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime InsertedDateTime { get; set; }
        public string? Type { get; set; }
    }
}
