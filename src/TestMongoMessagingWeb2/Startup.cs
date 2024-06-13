using Defender.Mongo.MessageBroker.Extensions;
using Defender.Mongo.MessageBroker.Configuration;
using TestBase.Repositories;
using TestBase.Services.Topic;
using TestBase.Model.Topic;
using TestBase.Model.Queue;
using TestBase.Services.Queue;

namespace TestMongoMessagingWeb2
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

            services.RegisterMessageBroker(configuration);

            services.Configure<PublisherOptions>(
                configuration.GetSection(nameof(PublisherOptions)));
            services.Configure<TestDBOptions>(
                configuration.GetSection(nameof(TestDBOptions)));

            services.AddSingleton<TestRepository<TextMessageT>>();
            services.AddSingleton<TestRepository<TextMessageQ>>();

            //services.AddHostedService<SaveTopicListener>();
            services.AddHostedService<BackgroundTopicListener>();
            services.AddHostedService<BackgroundTopicPublisher>();

            services.AddHostedService<BackgroundQueueRetryingListener>();
            services.AddHostedService<BackgroundQueueListener>();
            services.AddHostedService<BackgroundQueuePublisher>();

            services.AddTransient<MessagingService>();

            return services;
        }

    }
}