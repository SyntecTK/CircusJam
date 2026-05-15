using System;

public interface IEventBus 
{
    void Subscribe<T>(System.Action<T> handler);
    void Unsubscribe<T>(System.Action<T> handler);
    void Raise<T>(T eventData);
}
