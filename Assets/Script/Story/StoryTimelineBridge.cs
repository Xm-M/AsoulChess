using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 接收 Timeline Signal（stage 预制体上预绑 Signal Track 输出），转发给 <see cref="EnterMapPlugin_StoryStage"/>。
/// </summary>
public class StoryTimelineBridge : MonoBehaviour, INotificationReceiver
{
    readonly Dictionary<string, Action> _handlers = new Dictionary<string, Action>();

    public void ClearHandlers() => _handlers.Clear();

    public void Register(SignalAsset asset, Action handler)
    {
        if (asset == null || handler == null)
            return;
        _handlers[asset.name] = handler;
    }

    public void Register(string signalName, Action handler)
    {
        if (string.IsNullOrEmpty(signalName) || handler == null)
            return;
        _handlers[signalName] = handler;
    }

    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is not SignalEmitter emitter || emitter.asset == null)
            return;

        if (_handlers.TryGetValue(emitter.asset.name, out var handler))
            handler?.Invoke();
    }
}
