namespace Defender.Mongo.MessageBroker.Configuration;

public sealed record PublisherOptions
{
    public string? MessageText { get; set; } = "Default text";
    public int SleepTimeoutMs { get; set; } = 100;
}