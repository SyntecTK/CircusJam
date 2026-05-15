using System;
using System.Collections.Generic;

public class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public void Subscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if(!_handlers.ContainsKey(type))
            _handlers[type] = new List<Delegate>();
        _handlers[type].Add(handler);
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if(_handlers.TryGetValue(type, out var list))
            list.Remove(handler);
    }

    public void Raise<T>(T eventData)
    {
        var type = typeof(T);
        if(!_handlers.TryGetValue(type, out var list)) return;
        foreach (var handler in list.ToArray())
            ((Action<T>)handler)(eventData);
    }
}
