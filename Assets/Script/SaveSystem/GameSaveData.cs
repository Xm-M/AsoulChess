using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// 关卡存档根数据结构
/// </summary>
[Serializable]
public class GameSaveData
{
    /// <summary>
    /// 关卡唯一标识（使用 LevelData.sceneName）
    /// </summary>
    public string levelId;

    /// <summary>
    /// 存档时间戳
    /// </summary>
    public long saveTimestamp;

    /// <summary>
    /// 关卡进度
    /// </summary>
    public LevelSaveData levelData;

    /// <summary>
    /// 植物商店状态（选中的植物、阳光等）
    /// </summary>
    public PlantsShopSaveData plantsShopData;

    /// <summary>
    /// 场上玩家植物
    /// </summary>
    public List<ChessSaveData> playerPlants;

    /// <summary>
    /// 小推车状态（EnterWarPlugin_CarCreate）
    /// </summary>
    public List<CarSaveData> carsSaveData;

    /// <summary>
    /// 插件 Timer 存档（pluginId -> TimerSaveEntry）
    /// </summary>
    public List<TimerSaveEntry> timerSaveData;

    /// <summary>
    /// Buff 注册表：id → buffType，读档时按 id 取类型创建实例
    /// </summary>
    public List<BuffRegistryEntry> buffRegistry;

    public TimerSaveEntry GetOrCreateTimerEntry(string pluginId)
    {
        if (timerSaveData == null) timerSaveData = new List<TimerSaveEntry>();
        var e = timerSaveData.Find(x => x.pluginId == pluginId);
        if (e == null) { e = new TimerSaveEntry { pluginId = pluginId }; timerSaveData.Add(e); }
        return e;
    }

    public TimerSaveEntry GetTimerEntry(string pluginId)
    {
        if (timerSaveData == null) return null;
        return timerSaveData.Find(x => x.pluginId == pluginId);
    }
}

/// <summary>
/// 小推车存档数据
/// </summary>
[Serializable]
public class CarSaveData
{
    /// <summary>
    /// roomTile 中的索引
    /// </summary>
    public int roomTileIndex;

    /// <summary>
    /// 当前生命值
    /// </summary>
    public float hp;

    /// <summary>
    /// 最大生命值
    /// </summary>
    public float hpMax;

    /// <summary>
    /// creator 名称（小推车类型）
    /// </summary>
    public string creatorId;
}

/// <summary>
/// 关卡进度数据
/// </summary>
[Serializable]
public class LevelSaveData
{
    public int currentWave;
    public float t;
    public float mintime;
    public float maxtime;

    /// <summary>
    /// TimerManage.GameTime，读档时需恢复以正确还原 Timer
    /// </summary>
    public float gameTime;
}

/// <summary>
/// 插件 Timer 存档条目，供实现 ISaveableLevelPlugin 的插件使用
/// </summary>
[Serializable]
public class TimerSaveEntry
{
    public string pluginId;
    public float remainingTime;
    public bool isLoop;
    /// <summary>
    /// 插件自定义数据（如 CreateSunlight 的 n 值）
    /// </summary>
    public int extraInt;
}

/// <summary>
/// 植物商店存档数据
/// </summary>
[Serializable]
public class PlantsShopSaveData
{
    /// <summary>
    /// 选中的植物 creator 名称列表（对应 ShopIcon 的 good.chessName）
    /// </summary>
    public List<string> selectedCreatorIds;

    /// <summary>
    /// 当前阳光
    /// </summary>
    public int sunLight;
}

/// <summary>
/// 单个棋子存档数据
/// </summary>
[Serializable]
public class ChessSaveData
{
    /// <summary>
    /// PropertyCreator.chessName 或 creator.name
    /// </summary>
    public string creatorId;

    /// <summary>
    /// 所在格子坐标
    /// </summary>
    public int tileX;
    public int tileY;

    /// <summary>
    /// 当前生命值
    /// </summary>
    public float hp;

    /// <summary>
    /// 最大生命值
    /// </summary>
    public float hpMax;

    /// <summary>
    /// 该棋子身上的 Buff 列表（不含纯 Buff_BaseValueBuff）
    /// </summary>
    public List<BuffSaveData> buffs;
}

/// <summary>
/// Buff 注册表条目：id → buffType
/// </summary>
[Serializable]
public class BuffRegistryEntry
{
    public string id;
    public string buffType;
}

/// <summary>
/// 单个 Buff 存档数据：id、buffType、剩余时间、层数、数值、额外参数
/// </summary>
[Serializable]
public class BuffSaveData
{
    public string id;
    public string buffType;
    public float remainingTime = -1f;
    public int stackCount = 1;
    public List<string> valueKeys;
    public List<string> valueValues;
    /// <summary>额外参数键（层数、currentAttack、count、OutlineColor 等）</summary>
    public List<string> extraKeys;
    /// <summary>额外参数值（统一存 string，读取时按类型解析）</summary>
    public List<string> extraValues;

    public void SetValue(string key, float v) { Ensure(); valueKeys.Add(key); valueValues.Add(v.ToString()); }
    public void SetValue(string key, int v) { Ensure(); valueKeys.Add(key); valueValues.Add(v.ToString()); }
    public float GetValueFloat(string key, float def = 0) { if (valueKeys == null) return def; int i = valueKeys.IndexOf(key); if (i >= 0 && i < valueValues?.Count && float.TryParse(valueValues[i], out float v)) return v; return def; }
    public int GetValueInt(string key, int def = 0) { if (valueKeys == null) return def; int i = valueKeys.IndexOf(key); if (i >= 0 && i < valueValues?.Count && int.TryParse(valueValues[i], out int v)) return v; return def; }
    void Ensure() { if (valueKeys == null) valueKeys = new List<string>(); if (valueValues == null) valueValues = new List<string>(); }

    static readonly JsonSerializerSettings _extraSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

    /// <summary>用 Newtonsoft 序列化任意类型，支持 Color、Vector 等</summary>
    public void SetExtra(string key, object value)
    {
        EnsureExtra();
        extraKeys.Add(key);
        extraValues.Add(JsonConvert.SerializeObject(value, _extraSettings));
    }
    public void SetExtra(string key, float v) => SetExtra(key, (object)v);
    public void SetExtra(string key, int v) => SetExtra(key, (object)v);
    public void SetExtra(string key, bool v) => SetExtra(key, (object)v);
    public void SetExtra(string key, Color v)
    {
        EnsureExtra();
        extraKeys.Add(key);
        extraValues.Add($"{v.r.ToString(System.Globalization.CultureInfo.InvariantCulture)},{v.g.ToString(System.Globalization.CultureInfo.InvariantCulture)},{v.b.ToString(System.Globalization.CultureInfo.InvariantCulture)},{v.a.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
    }
    public void SetExtra(string key, Vector2 v) => SetExtra(key, (object)v);
    public void SetExtra(string key, Vector3 v) => SetExtra(key, (object)v);
    public void SetExtra(string key, string v) => SetExtra(key, (object)v);

    /// <summary>用 Newtonsoft 反序列化，可直接 as 转换：var c = data.GetExtra&lt;object&gt;(key) as Color</summary>
    public T GetExtra<T>(string key, T def = default)
    {
        if (extraKeys == null) return def;
        int i = extraKeys.IndexOf(key);
        if (i < 0 || i >= extraValues?.Count) return def;
        try
        {
            var obj = JsonConvert.DeserializeObject(extraValues[i], typeof(T), _extraSettings);
            if (obj == null) return def;
            return (T)obj;
        }
        catch { return def; }
    }
    public float GetExtraFloat(string key, float def = 0) => GetExtra(key, def);
    public int GetExtraInt(string key, int def = 0) => GetExtra(key, def);
    public bool GetExtraBool(string key, bool def = false) => GetExtra(key, def);
    public Color GetExtraColor(string key, Color def)
    {
        if (extraKeys == null) return def;
        int i = extraKeys.IndexOf(key);
        if (i < 0 || i >= extraValues?.Count) return def;
        var p = extraValues[i].Split(',');
        if (p.Length >= 4 && float.TryParse(p[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float r)
            && float.TryParse(p[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float g)
            && float.TryParse(p[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float b)
            && float.TryParse(p[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float a))
            return new Color(r, g, b, a);
        return def;
    }
    public Vector2 GetExtraVector2(string key, Vector2 def) => GetExtra(key, def);
    public Vector3 GetExtraVector3(string key, Vector3 def) => GetExtra(key, def);
    public string GetExtraString(string key, string def = null) => GetExtra<string>(key) ?? def;
    void EnsureExtra() { if (extraKeys == null) extraKeys = new List<string>(); if (extraValues == null) extraValues = new List<string>(); }
}
