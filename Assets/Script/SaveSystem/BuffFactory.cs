using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 反射收集 Buff 子类，按类名创建实例
/// </summary>
public static class BuffFactory
{
    private static Dictionary<string, Type> _typeMap;

    public static Buff Create(string buffType)
    {
        if (string.IsNullOrEmpty(buffType)) return null;
        EnsureTypes();
        if (_typeMap.TryGetValue(buffType, out var type))
            return Activator.CreateInstance(type) as Buff;
        Debug.LogWarning($"[BuffFactory] 未知 Buff 类型: {buffType}");
        return null;
    }

    private static void EnsureTypes()
    {
        if (_typeMap != null) return;
        _typeMap = new Dictionary<string, Type>(StringComparer.Ordinal);
        var buffBase = typeof(Buff);
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException e) { types = e.Types; }
            if (types == null) continue;
            foreach (var t in types)
            {
                if (t == null || t.IsAbstract || !buffBase.IsAssignableFrom(t)) continue;
                _typeMap[t.Name] = t;
            }
        }
    }
}
