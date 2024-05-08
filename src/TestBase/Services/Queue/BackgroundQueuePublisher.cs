using Defender.Mongo.MessageBroker.Configuration;
using Defender.Mongo.MessageBroker.Interfaces.Queue;
using Defender.Mongo.MessageBroker.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TestBase.Model.Queue;

namespace TestBase.Services.Queue;

public class BackgroundQueuePublisher : BackgroundService
{
    private readonly IQueueProducer _producer;
    private readonly PublisherOptions _options;

    public BackgroundQueuePublisher(IQueueProducer producer, IOptions<PublisherOptions> options)
    {
        _producer = producer.SetQueue(Queues.TextQueue);
        _producer = producer.SetMessageType(MessageType.ClassName);

        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int i = 100;
        while (!stoppingToken.IsCancellationRequested)
        {
            _producer.PublishQueueAsync(
                new TextMessage(
                    //$"{DateTime.UtcNow.ToShortDateString()}-{i++}-{_options.MessageText}"));                    
                    $"{i++}"));

            await Task.Delay(_options.SleepTimeoutMs, stoppingToken);
        }
    }
}
