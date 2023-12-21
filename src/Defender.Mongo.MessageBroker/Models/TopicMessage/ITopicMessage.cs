﻿using MongoDB.Bson.Serialization.Attributes;

namespace Defender.Mongo.MessageBroker.Models.TopicMessage
{
    public interface ITopicMessage
    {
        [BsonId]
        public Guid Id { get; set; }
        [BsonRepresentation(MongoDB.Bson.BsonType.DateTime)]
        public DateTime InsertedDateTime { get; set; }
        public string Type { get; set; }
    }
}
