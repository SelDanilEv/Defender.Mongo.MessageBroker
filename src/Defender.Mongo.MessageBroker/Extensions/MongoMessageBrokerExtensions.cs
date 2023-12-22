using Defender.Mongo.MessageBroker.Configuration;
using Defender.Mongo.MessageBroker.Interfaces;
using Defender.Mongo.MessageBroker.Interfaces.Private;
using Defender.Mongo.MessageBroker.Processing;
using Microsoft.Extensions.DependencyInjection;

namespace Defender.Mongo.MessageBroker.Extensions;

public static class MongoMessageBrokerExtensions
{
    public static IServiceCollection AddMongoMessageBrokerServices(
        this IServiceCollection services, 
        Action<MessageBrokerOptions> configuration)
    {
        services.Configure<MessageBrokerOptions>(configuration);

        services.AddSingleton<IMessageBroker, MongoMessageBroker>();

        services.AddTransient<IProducer, MessageProcessor>();

        services.AddTransient<IConsumer, MessageProcessor>();

        return services;
    }
}
