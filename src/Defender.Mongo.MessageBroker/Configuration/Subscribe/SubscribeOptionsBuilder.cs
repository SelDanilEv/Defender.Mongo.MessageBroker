using Defender.Mongo.MessageBroker.Models.Base;
using Defender.Mongo.MessageBroker.Models.TopicMessage;
using MongoDB.Driver;

namespace Defender.Mongo.MessageBroker.Configuration;

public record SubscribeOptionsBuilder<T> where T : IBaseMessage, new()
{
    private Func<T, Task<bool>>? _action { get; set; } = null;
    private Func<Task<DateTime>>? _startDateProvider { get; set; } = null;
    private Func<Task<FilterDefinition<T>>>? _filterProvider { get; set; } = null;

    public SubscribeOptions<T> Build()
    {
        return new SubscribeOptions<T>(
            _action ?? DefaultAction,
            _filterProvider ?? DefaultFilter,
            _startDateProvider ?? DefaultStartDate);
    }

    public static SubscribeOptionsBuilder<T> Create()
    {
        return new SubscribeOptionsBuilder<T>();
    }

    #region Defaults
    private static bool IsTopic => typeof(ITopicMessage).IsAssignableFrom(typeof(T));

    private static Func<T, Task<bool>> DefaultAction =>
        (T t) => Task.FromResult(true);

    private static Func<Task<FilterDefinition<T>>> DefaultFilter =>
        () => Task.FromResult(Builders<T>.Filter.Empty);

    private static Func<Task<DateTime>> DefaultStartDate =>
        () => Task.FromResult(IsTopic ? DateTime.UtcNow : DateTime.MinValue);

    #endregion

    #region Actions 

    public SubscribeOptionsBuilder<T> SetAction(Func<T, Task<bool>> action)
    {
        _action = action;
        return this;
    }

    public SubscribeOptionsBuilder<T> SetAction(Func<T, bool> action)
    {
        _action = ConvertToAsyncFunc(action);
        return this;
    }

    public SubscribeOptionsBuilder<T> SetAction(Func<T, Task> action)
    {
        _action = ConvertToAsyncFunc(action);
        return this;
    }

    public SubscribeOptionsBuilder<T> SetAction(Action<T> action)
    {
        _action = ConvertToAsyncFunc(action);
        return this;
    }

    #endregion

    #region Filters 

    public SubscribeOptionsBuilder<T> SetFilter(Func<Task<FilterDefinition<T>>> filter)
    {
        _filterProvider = filter;
        return this;
    }

    public SubscribeOptionsBuilder<T> SetFilter(Func<FilterDefinition<T>> filter)
    {
        _filterProvider = ConvertToAsyncFunc(filter);
        return this;
    }

    public SubscribeOptionsBuilder<T> SetFilter(FilterDefinition<T> filter)
    {
        _filterProvider = ConvertToAsyncFunc(filter);
        return this;
    }

    #endregion

    #region StartDate

    public SubscribeOptionsBuilder<T> SetStartDateTime(Func<Task<DateTime>> startDate)
    {
        _startDateProvider = startDate;
        return this;
    }

    public SubscribeOptionsBuilder<T> SetStartDateTime(Func<DateTime> startDate)
    {
        _startDateProvider = ConvertToAsyncFunc(startDate);
        return this;
    }

    public SubscribeOptionsBuilder<T> SetStartDateTime(DateTime dateTime)
    {
        _startDateProvider = ConvertToAsyncFunc(dateTime);
        return this;
    }

    #endregion

    #region Converters

    private static Func<Task<Q>> ConvertToAsyncFunc<Q>(Q model)
    {
        return () =>
        {
            return Task.FromResult(model);
        };
    }

    private static Func<Task<Q>> ConvertToAsyncFunc<Q>(Func<Q> model)
    {
        return () =>
        {
            return Task.FromResult(model());
        };
    }

    private static Func<Q, Task<bool>> ConvertToAsyncFunc<Q>(Func<Q, bool> action)
    {
        return (Q t) =>
        {
            var result = action(t);
            return Task.FromResult(result);
        };
    }

    private static Func<Q, Task<bool>> ConvertToAsyncFunc<Q>(Action<Q> action)
    {
        return (Q t) =>
        {
            action(t);
            return Task.FromResult(true);
        };
    }

    private static Func<Q, Task<bool>> ConvertToAsyncFunc<Q>(Func<Q, Task> func)
    {
        return (Q t) =>
        {
            func(t);
            return Task.FromResult(true);
        };
    }

    #endregion
}