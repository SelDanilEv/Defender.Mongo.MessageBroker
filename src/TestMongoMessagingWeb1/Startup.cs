using Defender.Mongo.MessageBroker.Extensions;
using Defender.Mongo.MessageBroker.Configuration;
using TestBase.Services;
using TestBase.Repositories;
using TestBase.Model;

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

            services.Configure<PublisherOptions>(
                configuration.GetSection(nameof(PublisherOptions)));

            services.AddSingleton<TestRepository<TextMessage>>();

            services.AddHostedService<SaveListener>();
            //services.AddHostedService<BackgroundPublisher>();
            services.AddTransient<MessagingService>();

            return services;
        }

    }
}