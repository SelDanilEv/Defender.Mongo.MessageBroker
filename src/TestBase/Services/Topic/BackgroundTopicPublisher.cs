using Defender.Mongo.MessageBroker.Interfaces.Topic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TestBase.Configuration;
using TestBase.Model.Topic;

namespace TestBase.Services.Topic;

public class BackgroundTopicPublisher : BackgroundService
{
    private readonly ITopicProducer<TextMessageT> _producer;
    private readonly PublisherOptions _options;

    public BackgroundTopicPublisher(ITopicProducer<TextMessageT> producer, IOptions<PublisherOptions> options)
    {
        _producer = producer;

        _options = options.Value;

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int i = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _producer.PublishTopicAsync(
                    new TextMessageT(
                    $"{DateTime.UtcNow.ToShortDateString()}-{i++}-{_options.MessageText}"));
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
