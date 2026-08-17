using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace AsoulChess.Game.Core.Events
{
    /// <summary>
    /// Lightweight event bus (from AVZ EventController, without EventName enum).
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        interface IHandlerEntry { }

        sealed class HandlerEntry : IHandlerEntry
        {
            public UnityAction Action;
        }

        sealed class HandlerEntry<T> : IHandlerEntry
        {
            public UnityAction<T> Action;
        }

        readonly Dictionary<string, IHandlerEntry> _handlers = new Dictionary<string, IHandlerEntry>();

        public void AddListener(string eventName, Action handler)
        {
            if (handler == null) return;
            AddListener(eventName, new UnityAction(handler));
        }

        public void AddListener(string eventName, UnityAction handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;
            if (!_handlers.TryGetValue(eventName, out var entry))
            {
                entry = new HandlerEntry();
                _handlers[eventName] = entry;
            }

            if (entry is HandlerEntry typed)
                typed.Action += handler;
            else
                throw new InvalidOperationException($"Event '{eventName}' already registered with a different payload type.");
        }

        public void AddListener<T>(string eventName, Action<T> handler)
        {
            if (handler == null) return;
            AddListener(eventName, new UnityAction<T>(handler));
        }

        public void AddListener<T>(string eventName, UnityAction<T> handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;
            if (!_handlers.TryGetValue(eventName, out var entry))
            {
                entry = new HandlerEntry<T>();
                _handlers[eventName] = entry;
            }

            if (entry is HandlerEntry<T> typed)
                typed.Action += handler;
            else
                throw new InvalidOperationException($"Event '{eventName}' already registered with a different payload type.");
        }

        public void RemoveListener(string eventName, Action handler)
        {
            if (handler == null) return;
            RemoveListener(eventName, new UnityAction(handler));
        }

        public void RemoveListener(string eventName, UnityAction handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;
            if (_handlers.TryGetValue(eventName, out var entry) && entry is HandlerEntry typed)
                typed.Action -= handler;
        }

        public void RemoveListener<T>(string eventName, Action<T> handler)
        {
            if (handler == null) return;
            RemoveListener(eventName, new UnityAction<T>(handler));
        }

        public void RemoveListener<T>(string eventName, UnityAction<T> handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null) return;
            if (_handlers.TryGetValue(eventName, out var entry) && entry is HandlerEntry<T> typed)
                typed.Action -= handler;
        }

        public void Trigger(string eventName)
        {
            if (string.IsNullOrEmpty(eventName)) return;
            if (_handlers.TryGetValue(eventName, out var entry) && entry is HandlerEntry typed)
                typed.Action?.Invoke();
        }

        public void Trigger<T>(string eventName, T payload)
        {
            if (string.IsNullOrEmpty(eventName)) return;
            if (_handlers.TryGetValue(eventName, out var entry) && entry is HandlerEntry<T> typed)
                typed.Action?.Invoke(payload);
        }

        public void Clear() => _handlers.Clear();
    }
}
