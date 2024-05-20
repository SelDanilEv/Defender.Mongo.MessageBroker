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
        _producer = producer.SetQueue(Queues.TextQueue)
            .SetMessageType(MessageType.ClassName)
            .SetMaxDocuments(1000)
            .SetMaxByteSize(int.MaxValue);

        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int i = 1000;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _producer.PublishQueueAsync(
                    new TextMessage(
                        //$"{DateTime.UtcNow.ToShortDateString()}-{i++}-{_options.MessageText}"));                    
                        $"{i++}"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                await Task.Delay(_options.SleepTimeoutMs, stoppingToken);
            }
        }
    }
}
