using Defender.Mongo.MessageBroker.Interfaces;
using Defender.Mongo.MessageBroker.Models.TopicMessage;
using Microsoft.Extensions.Hosting;
using TestBase.Model;

namespace TestBase.Services;

public class BackgroundListener : BackgroundService
{
    private readonly IConsumer _consumer;

    public BackgroundListener(IConsumer consumer)
    {
        _consumer = consumer;

        _consumer.SetTopic(Topics.TextTopic).SetMessageType(MessageType.ClassName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.SubscribeAsync<TextMessage>(
            (text) => Log.AddLog(text.Text),
            () => DateTime.UtcNow, 
            stoppingToken);
    }
}
