using System;
using System.Collections.Generic;

namespace RensaSimulator.events;

public class EventManager {
    private static EventManager _instance;
    public static EventManager Instance => _instance ??= new EventManager();

    private readonly Dictionary<Type, List<Delegate>> _eventHandlers = new Dictionary<Type, List<Delegate>>();

    public void Subscribe<T>(Action<T> handler) where T : IEvent {
        var eventType = typeof(T);
        if (!_eventHandlers.ContainsKey(eventType)) {
            _eventHandlers[eventType] = new List<Delegate>();
        }

        _eventHandlers[eventType].Add(handler);
    }

    public void Unsubscribe<T>(Action<T> handler) where T : IEvent {
        var eventType = typeof(T);
        if (_eventHandlers.ContainsKey(eventType)) {
            _eventHandlers[eventType].Remove(handler);
        }
    }

    public void Publish<T>(T eventItem) where T : IEvent {
        var eventType = typeof(T);
        if (_eventHandlers.TryGetValue(eventType, out var eventHandler)) {
            foreach (var handler in eventHandler) {
                ((Action<T>)handler)(eventItem);
            }
        }
    }
}