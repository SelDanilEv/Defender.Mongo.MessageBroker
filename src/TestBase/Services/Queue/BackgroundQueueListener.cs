using Defender.Mongo.MessageBroker.Interfaces.Queue;
using Defender.Mongo.MessageBroker.Models;
using Microsoft.Extensions.Hosting;
using TestBase.Model;
using TestBase.Model.Queue;
using TestBase.Repositories;

namespace TestBase.Services.Queue;

public class BackgroundQueueListener : BackgroundService
{
    private readonly TestRepository<TextMessage> _testRepository;
    private readonly IQueueConsumer _consumer;

    public BackgroundQueueListener(IQueueConsumer consumer,
        TestRepository<TextMessage> testRepository)
    {
        _testRepository = testRepository;
        _consumer = consumer;

        _consumer.SetQueue(Queues.TextQueue).SetMessageType(MessageType.ClassName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.SubscribeQueueAsync<TextMessage>(
            HandleTextMessage,
            stoppingToken);

    }

    private async Task<bool> HandleTextMessage(TextMessage text)
    {
        var result = new Random().Next(0, 100) <= 90;
        if (result)
        {
            Log.AddLog(text.Text);

            await _testRepository.Insert(text);
        }
        return result;
    }

}
