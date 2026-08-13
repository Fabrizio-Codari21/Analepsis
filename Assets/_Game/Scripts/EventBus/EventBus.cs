using System.Collections.Generic;
using UnityEngine;

public static class EventBus<T> where T : IEvent
{
    private static readonly HashSet<IEventBinding<T>>  Bindings = new HashSet<IEventBinding<T>>();
     
    public static void Register(EventBinding<T> binding)  => Bindings.Add(binding);
    public static void Unregister(EventBinding<T> binding)  => Bindings.Remove(binding);
    
    public static void Raise(T @event)
    {
        foreach (var binding in Bindings)
        {
            binding.OnEvent.Invoke(@event);
            binding.OnEventNoArgs.Invoke();
        }
    }

    public static void Clear()
    {
        Debug.Log($" Clearing {typeof(T).Name} bingdings");
        Bindings.Clear();
    }
}