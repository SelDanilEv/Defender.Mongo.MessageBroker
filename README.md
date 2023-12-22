# Defender.Mongo.MessageBroker

A message broker with topics based on Mongo, aiming to be a free and simplified version of Kafka under MongoDB.

# How to configure

1. Add the `Defender.Mongo.MessageBroker` NuGet package.
2. Call `AddMongoMessageBrokerServices` to register all the necessary services and options. Pass an `Action<MessageBrokerOptions>` as the first parameter to set the configuration.

```
services.AddMongoMessageBrokerServices(opt =>
{
    configuration.GetSection(nameof(MessageBrokerOptions)).Bind(opt);
});
```

> `MessageBrokerOptions` is required and has four parameters.
> `MongoDbConnectionString` - Connection string to your Mongo DB
> `MongoDbDatabaseName` - Database name
> `MaxTopicDocuments` - The maximum number of messages allowed in a topic (default = 1000).
> `MaxTopicByteSize` - The maximum size limit in bytes (default = 1000000, about 1 MB).

# How to use

Let's review the basic entities the library provides:

- `BaseTopicMessage`
- `ITopicMessage`
- `IConsumer`
- `IProducer`

## ITopicMessage and BaseTopicMessage
```
using Defender.Mongo.MessageBroker.Models.TopicMessage;
public class TextMessage : BaseTopicMessage
{
    public TextMessage() { }
    public TextMessage(string text) { Text = text; }
    public string Text { get; set; }
}
```
All models you publish and receive must inherit from one of these. You don't need to worry about the fields.

## IConsumer

```
public class BackgroundListener : BackgroundService
{
    private readonly IConsumer _consumer;
    public BackgroundListener(IConsumer consumer)
    {
        _consumer = consumer
            .SetTopic("topic-name")
            .SetMessageType(MessageType.ClassName);
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.SubscribeAsync<TextMessage>((text) => Log.AddLog(text.Text), stoppingToken);
    }
}
```

* Use `SetTopic` to specify the topic to subscribe to. By default, it is set to "default".
* Use `SetMessageType` to indicate the type of messages to listen to (it will skip other types). Possible values: `NoType` (subscribe to all), `ClassName` (set the type the same as the class name of the message), `Custom` (e.g., `.SetMessageType("custom-type"))`. By default, it's `NoType`.
* To subscribe to the topic, use `SubscribeAsync` and pass a delegate with a parameter to specify what to do with the message after it is received.

## IProducer

```
public class MessagingService
{
    private readonly IProducer _producer;
    public MessagingService(IProducer producer)
    {
        _producer = producer;
        _producer.SetTopic(Topics.TextTopic).SetMessageType(MessageType.ClassName);
    }
    public async Task PublishTextAsync(string text)
    {
        var message = new TextMessage(text);
        await _producer.PublishAsync(message);
    }
}
```

Everything is the same, but to publish a message, use the `PublishAsync` method.

# Behavior details

- Currently, it starts subscribing to the topic from the moment the application starts.
- It will create an instant connection to your MongoDB cluster and run a cursor (waiting for new documents in the collection).
- If something happens to the connection, it will constantly try to reconnect.
- After the connection is restored, it processes all messages that are later than the last message processed.
- If you set `MaxTopicDocuments` or `MaxTopicByteSize` too small, messages might be discarded and never be processed after a crash.
- If you need to ensure that only one service instance receives a message, create an auxiliary Consumer service that will be subscribed to the queue and will redirect the message to your service.
