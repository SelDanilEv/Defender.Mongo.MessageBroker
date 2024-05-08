using Defender.Mongo.MessageBroker.Extensions;
using Defender.Mongo.MessageBroker.Configuration;
using TestBase.Services.Topic;
using TestBase.Services.Queue;
using TestBase.Repositories;

namespace TestBase
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


            services.AddSingleton<TestRepository<TestBase.Model.Topic.TextMessage>>();
            services.AddSingleton<TestRepository<TestBase.Model.Queue.TextMessage>>();

            //services.AddHostedService<BackgroundListener>();
            //services.AddHostedService<BackgroundTopicPublisher>();
            services.AddHostedService<BackgroundQueueListener>();
            services.AddHostedService<BackgroundQueueRetryingListener>();

            services.AddTransient<MessagingService>();


            return services;
        }

    }
}