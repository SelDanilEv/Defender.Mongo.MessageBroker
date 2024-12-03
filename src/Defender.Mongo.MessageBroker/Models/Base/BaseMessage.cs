﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Defender.Mongo.MessageBroker.Models.Base
{
    public abstract record BaseMessage : IBaseMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public DateTime InsertedDateTime { get; set; }
        public string? Type { get; set; }
    }
}
