using Defender.Mongo.MessageBroker.Configuration.Subscribe;
using Defender.Mongo.MessageBroker.Interfaces.Topic;
using Microsoft.Extensions.Hosting;
using TestBase.Model;
using TestBase.Model.Topic;
using TestBase.Repositories;

namespace TestBase.Services.Topic;

public class SaveTopicListener : BackgroundService
{
    private readonly ITopicConsumer<TextMessageT> _consumer;
    private readonly TestRepository<TextMessageT> _testRepository;

    public SaveTopicListener(
        ITopicConsumer<TextMessageT> consumer,
        TestRepository<TextMessageT> testRepository)
    {
        _consumer = consumer;

        _testRepository = testRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = SubscribeOptionsBuilder<TextMessageT>
            .Create()
            .SetAction(async (text) =>
                {
                    Log.AddLog(text.Text);
                    text.Timestamp = DateTime.UtcNow;
                    await _testRepository.Insert(text);
                })
            .SetStartDateTime(DateTime.UtcNow);

        await _consumer.SubscribeTopicAsync(
            options.Build(),
            cancellationToken: stoppingToken);
    }
}
