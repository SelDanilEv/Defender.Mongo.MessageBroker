using Defender.Mongo.MessageBroker.Interfaces;
using Defender.Mongo.MessageBroker.Models.TopicMessage;
using Microsoft.Extensions.Hosting;
using TestBase.Model;
using TestBase.Repositories;

namespace TestBase.Services;

public class SaveListener : BackgroundService
{
    private readonly IConsumer _consumer;
    private readonly TestRepository<TextMessage> _testRepository;

    public SaveListener(
        IConsumer consumer,
        TestRepository<TextMessage> testRepository)
    {
        _consumer = consumer;

        _consumer.SetTopic(Topics.TextTopic).SetMessageType(MessageType.ClassName);

        _testRepository = testRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.SubscribeAsync<TextMessage>(
            async (text) =>
            {
                Log.AddLog(text.Text);
                text.ProcessedDateTime = DateTime.UtcNow;
                await _testRepository.Insert(text);
            },
            async () =>
            {
                var lastRecord = await _testRepository.GetLast();

                if (lastRecord == null)
                {
                    return DateTime.MinValue.AddMicroseconds(1);
                }

                return lastRecord?.ProcessedDateTime ?? DateTime.UtcNow;
            },
            stoppingToken);
    }
}
