using Defender.Mongo.MessageBroker.Extensions;
using Defender.Mongo.MessageBroker.Configuration;
using TestBase.Services;
using Microsoft.Extensions.DependencyInjection;

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

            services.AddHostedService<BackgroundListener>();
            //services.AddHostedService<BackgroundPublisher>();
            services.AddTransient<MessagingService>();

            return services;
        }

    }
}