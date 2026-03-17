using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基于用户名的玩家存档数据结构
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
