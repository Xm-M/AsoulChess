using System;
using System.Collections.Generic;

/// <summary>挥棒手接球回调注册表（避免技能 SO 实例字段存 per-Chess 状态）。</summary>
public static class BaseballBatterReceiveRegistry
{
    static readonly Dictionary<Chess, Action<Chess>> Receivers = new Dictionary<Chess, Action<Chess>>();

    public static void Register(Chess batter, Action<Chess> onReceive)
    {
        if (batter == null || onReceive == null)
            return;
        Receivers[batter] = onReceive;
    }

    public static void Unregister(Chess batter)
    {
        if (batter == null)
            return;
        Receivers.Remove(batter);
    }

    public static void TryReceive(Chess batter, Chess pitcher)
    {
        if (batter == null || !Receivers.TryGetValue(batter, out Action<Chess> handler))
            return;
        handler?.Invoke(pitcher);
    }
}
