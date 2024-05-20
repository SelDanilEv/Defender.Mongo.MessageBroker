using Defender.Mongo.MessageBroker.Interfaces;
using Defender.Mongo.MessageBroker.Interfaces.Topic;
using Defender.Mongo.MessageBroker.Models;
using Defender.Mongo.MessageBroker.Models.TopicMessage;
using Microsoft.Extensions.Hosting;
using TestBase.Model;
using TestBase.Model.Queue;
using TestBase.Model.Topic;
using TestBase.Repositories;
using TextMessage = TestBase.Model.Topic.TextMessage;

namespace TestBase.Services.Topic;

public class SaveTopicListener : BackgroundService
{
    private readonly ITopicConsumer _consumer;
    private readonly TestRepository<TextMessage> _testRepository;

    public SaveTopicListener(
        ITopicConsumer consumer,
        TestRepository<TextMessage> testRepository)
    {
        _consumer = consumer;

        _consumer.SetTopic(Topics.TextTopic).SetMessageType(MessageType.ClassName);

        _testRepository = testRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.SubscribeTopicAsync<TextMessage>(
            async (text) =>
            {
                Log.AddLog(text.Text);
                text.Timestamp = DateTime.UtcNow;
                await _testRepository.Insert(text);
            },
            async () =>
            {
                var lastRecord = await _testRepository.GetLast();

                if (lastRecord == null)
                {
                    return DateTime.MinValue.AddMicroseconds(1);
                }

                return lastRecord?.InsertedDateTime ?? DateTime.UtcNow;
            },
            stoppingToken);
    }
}
