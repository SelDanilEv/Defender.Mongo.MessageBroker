using Defender.Mongo.MessageBroker.Configuration;
using Defender.Mongo.MessageBroker.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TestBase.Model;

namespace TestBase.Services;

public class BackgroundPublisher : BackgroundService
{
    private readonly IProducer _producer;
    private readonly PublisherOptions _options;

    public BackgroundPublisher(IProducer producer, IOptions<PublisherOptions> options)
    {
        _producer = producer.SetTopic(Topics.TextTopic);

        _options = options.Value;

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int i = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            await _producer.PublishAsync<InvalidTextMessage>(new InvalidTextMessage($"{i}-{_options.MessageText}"));
            Thread.Sleep(_options.SleepTimeoutMs);
        }
    }
}
