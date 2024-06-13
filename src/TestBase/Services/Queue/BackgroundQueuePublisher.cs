using Defender.Mongo.MessageBroker.Configuration;
using Defender.Mongo.MessageBroker.Interfaces.Queue;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TestBase.Model.Queue;

namespace TestBase.Services.Queue;

public class BackgroundQueuePublisher : BackgroundService
{
    private readonly IQueueProducer<TextMessageQ> _producer;
    private readonly PublisherOptions _options;

    public BackgroundQueuePublisher(IQueueProducer<TextMessageQ> producer, IOptions<PublisherOptions> options)
    {
        _producer = producer;

        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int i = 10000;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _producer.PublishQueueAsync(
                    new TextMessageQ(
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
