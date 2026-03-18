using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单一玩家存档数据结构
/// </summary>
[Serializable]
public class PlayerSaveData
{
    public int saveVersion = 1;
    public string username;
    public long saveTimestamp;

    /// <summary>已通关关卡 levelName 列表</summary>
    public List<string> completedLevelIds;

    /// <summary>拥有的植物 creator.chessName 列表</summary>
    public List<string> ownedCreatorIds;

    /// <summary>金币（全局货币）</summary>
    public int coins;

    /// <summary>商店是否已解锁（车钥匙点击后为 true）</summary>
    public bool shopUnlocked;

    /// <summary>游戏设置</summary>
    /// <summary>BGM 音量 0~1</summary>
    public float bgmVolume = 1f;
    /// <summary>音效音量 0~1</summary>
    public float sfxVolume = 1f;
    /// <summary>分辨率宽度，0 表示使用默认</summary>
    public int screenWidth;
    /// <summary>分辨率高度，0 表示使用默认</summary>
    public int screenHeight;
    /// <summary>是否全屏</summary>
    public bool fullscreen = true;
    /// <summary>难度 0=简单 1=普通 2=困难，后续可扩展</summary>
    public int difficultyLevel;

    /// <summary>扩展数据，方便后续新增字段</summary>
    public List<PlayerSaveExtraEntry> extras;

    public PlayerSaveData()
    {
        completedLevelIds = new List<string>();
        ownedCreatorIds = new List<string>();
        extras = new List<PlayerSaveExtraEntry>();
    }
}

/// <summary>
/// 扩展数据条目，用于后续新增保存内容
/// </summary>
[Serializable]
public class PlayerSaveExtraEntry
{
    public string key;
    public string type;   // "int", "float", "string", "json"
    public string value;
}
