using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 教程关卡行数限制：隐藏非允许行，重排 tiles，使 mapPos 从 0 开始连续。
/// 需放在 EnterMapPlugin 列表首位，在 CarCreate 之前执行。
/// 场景重载时自动恢复。
/// </summary>
public class EnterWarPlugin_TutorialRows : ILevelPlugin
{
    [Tooltip("1=单行 3=三行，0 或负数=不限制")]
    public int maxRowCount = 3;
    public Sprite sprite;
    public void StadgeEffect(LevelController levelController)
    {
        if (maxRowCount <= 0) return;

        var map = MapManage.instance;
        var mapPvz = map as MapManage_PVZ;
        if (map == null || mapPvz == null || map.tiles == null) return;

        int totalRows = map.mapSize.y;
        if (maxRowCount >= totalRows) return;

        var keptRows = GetKeptRows(totalRows, maxRowCount);
        if (keptRows.Count == 0) return;

        // 1. 隐藏非保留行
        for (int x = 0; x < map.mapSize.x; x++)
        {
            for (int y = 0; y < totalRows; y++)
            {
                if (!keptRows.Contains(y))
                    map.tiles[x, y].gameObject.SetActive(false);
            }
        }

        // 2. 备份原 tiles 后重排
        int sizeX = map.mapSize.x;
        var origTiles = new Tile[sizeX, totalRows];
        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < totalRows; y++)
                origTiles[x, y] = map.tiles[x, y];

        for (int x = 0; x < sizeX; x++)
        {
            for (int newY = 0; newY < keptRows.Count; newY++)
            {
                var t = origTiles[x, keptRows[newY]];
                t.mapPos = new Vector2Int(x, newY);
                map.tiles[x, newY] = t;
            }
        }

        // 3. 更新 mapSize
        map.mapSize = new Vector2Int(map.mapSize.x, keptRows.Count);

        // 4. 重建 preTiles
        map.preTiles.Clear();
        for (int i = 0; i < map.mapSize.y; i++)
            map.preTiles.Add(map.tiles[map.mapSize.x - 1, i]);

        // 5. 重建 roomTile（使用原 roomTile 中对应行的 tile，不足时用 tiles[0,i]）
        var newRoomTile = new List<Tile>();
        for (int i = 0; i < keptRows.Count; i++)
        {
            int origY = keptRows[i];
            if (mapPvz.roomTile != null && origY < mapPvz.roomTile.Count)
                newRoomTile.Add(mapPvz.roomTile[origY]);
            else
                newRoomTile.Add(map.tiles[0, i]);
        }
        if (sprite != null) MapManage.instance.backGround.sprite = sprite;
        mapPvz.roomTile.Clear();
        mapPvz.roomTile.AddRange(newRoomTile);
    }

    public void OverPlugin(LevelController levelController)
    {
        // 场景重载会恢复，无需清理
    }

    /// <summary>获取保留的行索引（居中）：1 行取中间，3 行取中间三行</summary>
    static List<int> GetKeptRows(int totalRows, int count)
    {
        var list = new List<int>();
        for (int i = 0; i < count; i++)
        {
            int origY = (totalRows - count) / 2 + i;
            if (origY >= 0 && origY < totalRows)
                list.Add(origY);
        }
        return list;
    }
}
