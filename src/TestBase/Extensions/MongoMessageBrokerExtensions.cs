using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestBase.Consts;
using TestBase.Model.Queue;
using TestBase.Model.Topic;

namespace Defender.Mongo.MessageBroker.Extensions;

public static class RegisterMessageBrokerExtensions
{
    public static IServiceCollection RegisterMessageBroker(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddQueueConsumer<TextMessageQ>(opt =>
        {
            opt.MongoDbConnectionString = configuration[StaticConsts.MessageBrokerAdminConnectionString];
            opt.MongoDbDatabaseName = StaticConsts.DBName;
            opt.Name = Queues.TextQueue;
            opt.MaxDocuments = StaticConsts.QueueDocuments;
            opt.MaxByteSize = StaticConsts.QueueMaxByteSize;
        });

        services.AddQueueProducer<TextMessageQ>(opt =>
        {
            opt.MongoDbConnectionString = configuration[StaticConsts.MessageBrokerAdminConnectionString];
            opt.MongoDbDatabaseName = StaticConsts.DBName;
            opt.Name = Queues.TextQueue;
            opt.MaxDocuments = StaticConsts.QueueDocuments;
            opt.MaxByteSize = StaticConsts.QueueMaxByteSize;
        });

        services.AddTopicConsumer<TextMessageT>(opt =>
        {
            opt.MongoDbConnectionString = configuration[StaticConsts.MessageBrokerROConnectionString];
            opt.MongoDbDatabaseName = StaticConsts.DBName;
            opt.Name = Topics.TextTopic;
            opt.MaxDocuments = StaticConsts.TopicDocuments;
            opt.MaxByteSize = StaticConsts.TopicMaxByteSize;
        });

        services.AddTopicProducer<TextMessageT>(opt =>
        {
            opt.MongoDbConnectionString = configuration[StaticConsts.MessageBrokerAdminConnectionString];
            opt.MongoDbDatabaseName = StaticConsts.DBName;
            opt.Name = Topics.TextTopic;
            opt.MaxDocuments = StaticConsts.TopicDocuments;
            opt.MaxByteSize = StaticConsts.TopicMaxByteSize;
        });




        services.AddQueueConsumer<InvalidTextMessageQ>(opt =>
        {
            opt.MongoDbConnectionString = configuration[StaticConsts.MessageBrokerROConnectionString];
            opt.MongoDbDatabaseName = StaticConsts.DBName;
            opt.Name = Queues.TextQueue;
            opt.MaxDocuments = StaticConsts.QueueDocuments;
            opt.MaxByteSize = StaticConsts.QueueMaxByteSize;
        });

        services.AddQueueProducer<InvalidTextMessageQ>(opt =>
        {
            opt.MongoDbConnectionString = configuration[StaticConsts.MessageBrokerAdminConnectionString];
            opt.MongoDbDatabaseName = StaticConsts.DBName;
            opt.Name = Queues.TextQueue;
            opt.MaxDocuments = StaticConsts.QueueDocuments;
            opt.MaxByteSize = StaticConsts.QueueMaxByteSize;
        });

        services.AddTopicConsumer<InvalidTextMessageT>(opt =>
        {
            opt.MongoDbConnectionString = configuration[StaticConsts.MessageBrokerROConnectionString];
            opt.MongoDbDatabaseName = StaticConsts.DBName;
            opt.Name = Topics.TextTopic;
            opt.MaxDocuments = StaticConsts.TopicDocuments;
            opt.MaxByteSize = StaticConsts.TopicMaxByteSize;
        });

        services.AddTopicProducer<InvalidTextMessageT>(opt =>
        {
            opt.MongoDbConnectionString = configuration[StaticConsts.MessageBrokerAdminConnectionString];
            opt.MongoDbDatabaseName = StaticConsts.DBName;
            opt.Name = Topics.TextTopic;
            opt.MaxDocuments = StaticConsts.TopicDocuments;
            opt.MaxByteSize = StaticConsts.TopicMaxByteSize;
        });

        return services;
    }
}
