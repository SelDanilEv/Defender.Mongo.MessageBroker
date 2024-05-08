namespace Defender.Mongo.MessageBroker.Configuration;

public sealed record MessageBrokerOptions
{
    public string? MongoDbConnectionString { get; set; }
    public string? MongoDbDatabaseName { get; set; }
    public long MaxTopicDocuments { get; set; } = 2000;
    public long MaxTopicByteSize { get; set; } = 1000000;
    public long MaxQueueDocuments { get; set; } = 2000;
    public long MaxQueueByteSize { get; set; } = 1000000;
}