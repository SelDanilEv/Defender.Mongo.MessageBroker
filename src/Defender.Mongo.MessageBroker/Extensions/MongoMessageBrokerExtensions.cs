using Defender.Mongo.MessageBroker.Configuration;
using Defender.Mongo.MessageBroker.DB;
using Defender.Mongo.MessageBroker.Implementation;
using Defender.Mongo.MessageBroker.Interfaces.Queue;
using Defender.Mongo.MessageBroker.Interfaces.Topic;
using Defender.Mongo.MessageBroker.Models.Base;
using Defender.Mongo.MessageBroker.Models.QueueMessage;
using Defender.Mongo.MessageBroker.Models.TopicMessage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Defender.Mongo.MessageBroker.Extensions;

public static class MongoMessageBrokerExtensions
{
    public static IServiceCollection AddQueueConsumer<T>(
        this IServiceCollection services,
        Action<MessageBrokerOptions<T>> configureOptions) where T : IQueueMessage, new()
    {
        var name = services.RegisterUniqueOptions(configureOptions);

        services.AddTransient<IQueueConsumer<T>>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptionsSnapshot<MessageBrokerOptions<T>>>().Get(name);
            var broker = new MongoMessageBroker<T>(options);
            return new QueueMessageProcessor<T>(broker);
        });

        return services;
    }

    public static IServiceCollection AddQueueProducer<T>(
        this IServiceCollection services,
        Action<MessageBrokerOptions<T>> configureOptions) where T : IQueueMessage, new()
    {
        var name = services.RegisterUniqueOptions(configureOptions);

        services.AddTransient<IQueueProducer<T>>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptionsSnapshot<MessageBrokerOptions<T>>>().Get(name);
            var broker = new MongoMessageBroker<T>(options);
            return new QueueMessageProcessor<T>(broker);
        });

        return services;
    }

    public static IServiceCollection AddTopicConsumer<T>(
        this IServiceCollection services,
        Action<MessageBrokerOptions<T>> configureOptions) where T : ITopicMessage, new()
    {
        var name = services.RegisterUniqueOptions(configureOptions);

        services.AddTransient<ITopicConsumer<T>>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptionsSnapshot<MessageBrokerOptions<T>>>().Get(name);
            var broker = new MongoMessageBroker<T>(options);
            return new TopicMessageProcessor<T>(broker);
        });

        return services;
    }

    public static IServiceCollection AddTopicProducer<T>(
        this IServiceCollection services,
        Action<MessageBrokerOptions<T>> configureOptions) where T : ITopicMessage, new()
    {
        var name = services.RegisterUniqueOptions(configureOptions);

        services.AddTransient<ITopicProducer<T>>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptionsSnapshot<MessageBrokerOptions<T>>>().Get(name);
            var broker = new MongoMessageBroker<T>(options);
            return new TopicMessageProcessor<T>(broker);
        });

        return services;
    }

    private static string RegisterUniqueOptions<T>(
        this IServiceCollection services,
        Action<MessageBrokerOptions<T>> configureOptions) where T : IBaseMessage, new()
    {
        var name = Guid.NewGuid().ToString();

        services.Configure<MessageBrokerOptions<T>>(name, configureOptions);

        return name;
    }
}
