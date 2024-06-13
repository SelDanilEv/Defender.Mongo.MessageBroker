using Defender.Mongo.MessageBroker.Configuration;
using Defender.Mongo.MessageBroker.Interfaces.Queue;
using Microsoft.Extensions.Hosting;
using TestBase.Model;
using TestBase.Model.Queue;
using TestBase.Repositories;

namespace TestBase.Services.Queue;

public class BackgroundQueueListener : BackgroundService
{
    private readonly TestRepository<TextMessageQ> _testRepository;
    private readonly IQueueConsumer<TextMessageQ> _consumer;

    public BackgroundQueueListener(IQueueConsumer<TextMessageQ> consumer,
        TestRepository<TextMessageQ> testRepository)
    {
        _testRepository = testRepository;
        _consumer = consumer;

        //.SetQueue(Queues.TextQueue)
        //.SetMessageType(MessageType.ClassName)
        //.SetMaxDocuments(1000)
        //.SetMaxByteSize(int.MaxValue);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var options = SubscribeOptionsBuilder<TextMessageQ>
                .Create()
                .SetAction(HandleTextMessage);

            await _consumer.SubscribeQueueAsync(
                options.Build(),
                cancellationToken: stoppingToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async Task<bool> HandleTextMessage(TextMessageQ text)
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
