using System;

namespace AsoulChess.Game.Core.Events
{
    /// <summary>String-keyed event bus. Game-specific event names stay in the game project.</summary>
    public interface IEventBus
    {
        void AddListener(string eventName, Action handler);
        void AddListener<T>(string eventName, Action<T> handler);
        void RemoveListener(string eventName, Action handler);
        void RemoveListener<T>(string eventName, Action<T> handler);
        void Trigger(string eventName);
        void Trigger<T>(string eventName, T payload);
        void Clear();
    }
}
