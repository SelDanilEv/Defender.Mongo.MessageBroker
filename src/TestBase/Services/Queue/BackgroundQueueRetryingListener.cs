using Defender.Mongo.MessageBroker.Configuration.Subscribe;
using Defender.Mongo.MessageBroker.Interfaces.Queue;
using Microsoft.Extensions.Hosting;
using TestBase.Model;
using TestBase.Model.Queue;
using TestBase.Repositories;

namespace TestBase.Services.Queue;

public class BackgroundQueueRetryingListener : IHostedService, IDisposable
{
    private readonly IQueueConsumer<TextMessageQ> _consumer;
    private readonly TestRepository<TextMessageQ> _testRepository;
    private Timer? _timer;
    private bool _isRunning = false;

    public BackgroundQueueRetryingListener(
        IQueueConsumer<TextMessageQ> consumer,
        TestRepository<TextMessageQ> testRepository)
    {
        _testRepository = testRepository;
        _consumer = consumer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(async _ => await Retry(null, cancellationToken), null, TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    private async Task Retry(object? state, CancellationToken stoppingToken)
    {
        if (_isRunning)
        {
            return;
        }
        _isRunning = true;

        var options = SubscribeOptionsBuilder<TextMessageQ>
            .Create()
            .SetAction(HandleTextMessage);

        await _consumer.RetryMissedEventsAsync(options.Build(), stoppingToken);

        _isRunning = false;
    }

    private async Task<bool> HandleTextMessage(TextMessageQ text)
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
