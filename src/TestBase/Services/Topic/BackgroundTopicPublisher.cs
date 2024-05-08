using Defender.Mongo.MessageBroker.Configuration;
using Defender.Mongo.MessageBroker.Interfaces;
using Defender.Mongo.MessageBroker.Interfaces.Topic;
using Defender.Mongo.MessageBroker.Models;
using Defender.Mongo.MessageBroker.Models.TopicMessage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TestBase.Model.Topic;

namespace TestBase.Services.Topic;

public class BackgroundTopicPublisher : BackgroundService
{
    private readonly ITopicProducer _producer;
    private readonly PublisherOptions _options;

    public BackgroundTopicPublisher(ITopicProducer producer, IOptions<PublisherOptions> options)
    {
        _producer = producer.SetTopic(Topics.TextTopic);
        _producer = producer.SetMessageType(MessageType.ClassName);

        _options = options.Value;

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int i = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            await _producer.PublishTopicAsync(
                new TextMessage(
                    $"{DateTime.UtcNow.ToShortDateString()}-{i}-{_options.MessageText}"));
            Thread.Sleep(_options.SleepTimeoutMs);
        }
    }
}
