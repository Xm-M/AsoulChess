using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 开局按队列提前放置单位：每条为 PropertyCreator + 格子坐标 + 阵营 tag。
/// 读档时跳过（场上单位由存档恢复）。格子无效或 creator 为空则跳过该条。
/// </summary>
[Serializable]
public class PrePlaceChessEntry
{
    [LabelText("单位（PropertyCreator）")]
    public PropertyCreator creator;

    [LabelText("格子坐标 (x,y)")]
    public Vector2Int pos;

    [LabelText("阵营 Tag")]
    [Tooltip("通常 Player / Enemy")]
    public string tag = "Player";
}

[Serializable]
public class GameStartPlugin_PrePlaceChess : ILevelPlugin
{
    [LabelText("预放置队列")]
    public List<PrePlaceChessEntry> entries = new List<PrePlaceChessEntry>();

    public void StadgeEffect(LevelController levelController)
    {
        if (SaveLoadContext.IsLoadFromSave)
            return;

        if (entries == null || entries.Count == 0)
            return;

        var map = MapManage.instance;
        if (map == null || map.tiles == null || ChessTeamManage.Instance == null)
            return;

        for (int i = 0; i < entries.Count; i++)
        {
            PrePlaceChessEntry entry = entries[i];
            if (entry == null || entry.creator == null)
                continue;

            Vector2Int pos = entry.pos;
            if (!map.IfInMapRange(pos.x, pos.y))
                continue;

            Tile tile = map.tiles[pos.x, pos.y];
            if (tile == null)
                continue;

            string teamTag = string.IsNullOrEmpty(entry.tag) ? "Player" : entry.tag;
            Chess chess = ChessTeamManage.Instance.CreateChess(entry.creator, tile, teamTag);
            if (chess == null)
                continue;

            tile.PlantChess(chess);
        }
    }

    public void OverPlugin(LevelController levelController)
    {
        // 单位随队伍 / 离场清理，无需额外销毁
    }
}
