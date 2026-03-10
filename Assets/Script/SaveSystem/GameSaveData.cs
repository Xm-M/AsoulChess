using System;
using System.Collections.Generic;
using UnityEngine;

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
}
