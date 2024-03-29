﻿using Defender.Mongo.MessageBroker.Models.TopicMessage;
using MongoDB.Driver;

namespace Defender.Mongo.MessageBroker.Helpers
{
    public static class EventHelper
    {
        public static async Task<T> GetLastProccedEvent<T>(
            this IMongoCollection<T> collection, FilterDefinition<T>? filter = null)
            where T : ITopicMessage
        {
            filter ??= Builders<T>.Filter.Empty;
            var sort = Builders<T>.Sort.Descending(x => x.InsertedDateTime);

            var lastProccedEvent = await collection
                .Find(filter)
                .Sort(sort)
                .Limit(1)
                .FirstOrDefaultAsync();

            return lastProccedEvent;
        }
    }
}
