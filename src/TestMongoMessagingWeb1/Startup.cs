using Defender.Mongo.MessageBroker.Extensions;
using Defender.Mongo.MessageBroker.Configuration;
using TestBase.Repositories;
using TestBase.Services.Topic;
using TestBase.Model.Topic;
using TestBase.Model.Queue;
using TestBase.Services.Queue;

namespace TestMongoMessagingWeb1
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddWebServices(
            this IServiceCollection services,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddMongoMessageBrokerServices(opt =>
            {
                configuration.GetSection(nameof(MessageBrokerOptions)).Bind(opt);
            });

            services.Configure<PublisherOptions>(
                configuration.GetSection(nameof(PublisherOptions)));

            services.AddSingleton<TestRepository<TestBase.Model.Topic.TextMessage>>();
            services.AddSingleton<TestRepository<TestBase.Model.Queue.TextMessage>>();

            //services.AddHostedService<SaveTopicListener>();
            //services.AddHostedService<BackgroundPublisher>();
            //services.AddHostedService<BackgroundQueueRetryingListener>();
            services.AddHostedService<BackgroundQueueListener>();
            services.AddHostedService<BackgroundQueuePublisher>();

            services.AddTransient<MessagingService>();

            return services;
        }

    }
}