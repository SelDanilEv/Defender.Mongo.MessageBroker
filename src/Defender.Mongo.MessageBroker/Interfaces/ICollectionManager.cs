using Defender.Mongo.MessageBroker.Interfaces.Topic;

namespace Defender.Mongo.MessageBroker.Interfaces;

public interface ICollectionManager
{
    Task EnsureCollectionExistsAsync();
}
