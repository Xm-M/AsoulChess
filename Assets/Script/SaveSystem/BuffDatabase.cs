using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Buff 注册表：存档时收集 id→buffType，读档时按 id 取类型
/// </summary>
public static class BuffDatabase
{
    private static Dictionary<string, string> _registry = new Dictionary<string, string>();

    /// <summary>
    /// 存档时采集：遍历所有棋子的 Buff，收集非 Buff_BaseValueBuff 的 id→buffType
    /// </summary>
    public static List<BuffRegistryEntry> CaptureRegistry(List<ChessSaveData> playerPlants)
    {
        var seen = new Dictionary<string, string>();
        if (playerPlants == null) return new List<BuffRegistryEntry>();
        foreach (var p in playerPlants)
        {
            if (p.buffs == null) continue;
            foreach (var b in p.buffs)
            {
                if (b == null || string.IsNullOrEmpty(b.id)) continue;
                if (!seen.ContainsKey(b.id) && !string.IsNullOrEmpty(b.buffType))
                    seen[b.id] = b.buffType;
            }
        }
        var list = new List<BuffRegistryEntry>();
        foreach (var kv in seen)
            list.Add(new BuffRegistryEntry { id = kv.Key, buffType = kv.Value });
        return list;
    }

    /// <summary>
    /// 读档时恢复：建立 id→buffType 映射
    /// </summary>
    public static void RestoreRegistry(List<BuffRegistryEntry> entries)
    {
        _registry.Clear();
        if (entries == null) return;
        foreach (var e in entries)
        {
            if (e == null || string.IsNullOrEmpty(e.id) || string.IsNullOrEmpty(e.buffType)) continue;
            _registry[e.id] = e.buffType;
        }
    }

    /// <summary>
    /// 按 id 获取 buffType
    /// </summary>
    public static string GetBuffType(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return _registry.TryGetValue(id, out var t) ? t : null;
    }

    public static void Clear()
    {
        _registry.Clear();
    }
}
