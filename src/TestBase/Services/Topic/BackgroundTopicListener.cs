using Defender.Mongo.MessageBroker.Configuration.Subscribe;
using Defender.Mongo.MessageBroker.Interfaces.Topic;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using TestBase.Model;
using TextMessageT = TestBase.Model.Topic.TextMessageT;

namespace TestBase.Services.Topic;

public class BackgroundTopicListener : BackgroundService
{
    private readonly ITopicConsumer<TextMessageT> _consumer;

    public BackgroundTopicListener(ITopicConsumer<TextMessageT> consumer)
    {
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = SubscribeOptionsBuilder<TextMessageT>
            .Create()
            .SetAction((text) => Log.AddLog(text.Text))
            .SetFilter(FilterDefinition<TextMessageT>.Empty)
            .SetStartDateTime(DateTime.UtcNow);

        await _consumer.SubscribeTopicAsync(
            options.Build(),
            stoppingToken);
    }
}
