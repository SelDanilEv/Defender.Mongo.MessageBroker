namespace Defender.Mongo.MessageBroker.Configuration;

public sealed record MessageBrokerOptions
{
    public string? MongoDbConnectionString { get; set; }
    public string? MongoDbDatabaseName { get; set; }
    public long MaxTopicDocuments { get; set; } = 1000;
    public long MaxTopicByteSize { get; set; } = 1000000;
}