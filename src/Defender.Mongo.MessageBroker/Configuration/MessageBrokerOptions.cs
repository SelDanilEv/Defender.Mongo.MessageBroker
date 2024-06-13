using Defender.Mongo.MessageBroker.Models.Base;

namespace Defender.Mongo.MessageBroker.Configuration;

public record MessageBrokerOptions<T> where T: IBaseMessage, new()
{
    public string? MongoDbConnectionString { get; set; }
    public string? MongoDbDatabaseName { get; set; }
    public long MaxDocuments { get; set; } = 2000;
    public long MaxByteSize { get; set; } = 1000000;
    public string Type { get; set; } = typeof(T).Name;
    public string Name { get; set; } = "default";

    public MessageBrokerOptions<T> ApplyOptions(MessageBrokerOptions<T> options)
    {
        this.MongoDbConnectionString = options.MongoDbConnectionString;
        this.MongoDbDatabaseName = options.MongoDbDatabaseName;
        this.MaxDocuments = options.MaxDocuments;
        this.MaxByteSize = options.MaxByteSize;
        this.Type = options.Type;
        this.Name = options.Name;

        return this;
    }
}