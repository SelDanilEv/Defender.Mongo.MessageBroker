namespace Defender.Mongo.MessageBroker.Configuration;

public sealed record TestDBOptions
{
    public string? MongoDbConnectionString { get; set; }
    public string? MongoDbDatabaseName { get; set; } = "test-db";

}