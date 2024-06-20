using Defender.Mongo.MessageBroker.Models.Base;
using MongoDB.Driver;

namespace Defender.Mongo.MessageBroker.Configuration.Subscribe;

public record SubscribeOptions<T> where T : IBaseMessage, new()
{
    public SubscribeOptions(
        Func<T, Task<bool>> action,
        Func<Task<FilterDefinition<T>>> filterProvider,
        Func<Task<DateTime>> startDateProvider)
    {
        Action = action;
        FilterProvider = filterProvider;
        StartDateProvider = startDateProvider;
    }

    public Func<T, Task<bool>> Action { get; init; }
    public Func<Task<FilterDefinition<T>>> FilterProvider { get; init; }
    public Func<Task<DateTime>> StartDateProvider { get; init; }
}