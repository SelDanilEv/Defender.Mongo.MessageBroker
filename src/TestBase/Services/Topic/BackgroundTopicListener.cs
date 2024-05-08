using Defender.Mongo.MessageBroker.Interfaces;
using Defender.Mongo.MessageBroker.Interfaces.Topic;
using Defender.Mongo.MessageBroker.Models;
using Defender.Mongo.MessageBroker.Models.TopicMessage;
using Microsoft.Extensions.Hosting;
using TestBase.Model;
using TestBase.Model.Queue;
using TestBase.Model.Topic;
using TextMessage = TestBase.Model.Topic.TextMessage;

namespace TestBase.Services.Topic;

public class BackgroundTopicListener : BackgroundService
{
    private readonly ITopicConsumer _consumer;

    public BackgroundTopicListener(ITopicConsumer consumer)
    {
        _consumer = consumer;

        _consumer.SetTopic(Topics.TextTopic).SetMessageType(MessageType.ClassName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.SubscribeTopicAsync<TextMessage>(
            (text) => Log.AddLog(text.Text),
            () => DateTime.UtcNow,
            stoppingToken);
    }
}
