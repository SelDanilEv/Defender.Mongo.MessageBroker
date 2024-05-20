using Defender.Mongo.MessageBroker.Interfaces.Queue;
using Defender.Mongo.MessageBroker.Models;
using Microsoft.Extensions.Hosting;
using TestBase.Model;
using TestBase.Model.Queue;
using TestBase.Repositories;

namespace TestBase.Services.Queue;

public class BackgroundQueueRetryingListener : IHostedService, IDisposable
{
    private readonly IQueueConsumer _consumer;
    private readonly TestRepository<TextMessage> _testRepository;
    private Timer? _timer;
    private bool _isRunning = false;

    public BackgroundQueueRetryingListener(
        IQueueConsumer consumer, 
        TestRepository<TextMessage> testRepository)
    {
        _testRepository = testRepository;
        _consumer = consumer.SetQueue(Queues.TextQueue)
            .SetMessageType(MessageType.ClassName)
            .SetMaxDocuments(1000)
            .SetMaxByteSize(int.MaxValue);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(async _ => await Retry(null, cancellationToken), null, TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    private async Task Retry(object? state, CancellationToken stoppingToken)
    {
        if(_isRunning)
        {
            return;
        }
        _isRunning = true;
        await _consumer.CheckQueueAsync<TextMessage>(HandleTextMessage, stoppingToken);
        _isRunning = false;
    }

    private async Task<bool> HandleTextMessage(TextMessage text)
    {
        var result = new Random().Next(0, 100) <= 100;
        if (result)
        {
            Log.AddLog(text.Text);

            await _testRepository.Insert(text);
        }
        return result;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
