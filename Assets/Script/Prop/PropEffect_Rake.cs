using System;
using UnityEngine;

/// <summary>
/// 钉耙：本局第一只敌方棋子入场时，在其所在行最左可种植格生成钉耙（秒杀第一只踩到的僵尸）。
/// </summary>
[Serializable]
public class PropEffect_Rake : PropEffect
{
    public GameObject rakePrefab;

    bool listening;
    bool placed;
    TileEffect_Rake activeRake;

    public override void Apply(PropContext ctx)
    {
        placed = false;
        if (listening) return;
        EventController.Instance.AddListener<Chess>(EventName.WhenChessEnterWar.ToString(), OnChessEnterWar);
        listening = true;
    }

    public override void Remove(PropContext ctx)
    {
        if (listening)
        {
            EventController.Instance.RemoveListener<Chess>(EventName.WhenChessEnterWar.ToString(), OnChessEnterWar);
            listening = false;
        }
        placed = false;
        if (activeRake != null)
        {
            activeRake.LeaveTile(FindTileHostingRake(activeRake));
            activeRake = null;
        }
    }

    void OnChessEnterWar(Chess chess)
    {
        if (placed || chess == null || chess.IfDeath || !chess.CompareTag("Enemy")) return;
        var stand = chess.moveController != null ? chess.moveController.standTile : null;
        if (stand == null) return;
        TrySpawnOnRow(stand.mapPos.y);
        placed = true;
    }

    void TrySpawnOnRow(int rowY)
    {
        if (rakePrefab == null || ObjectPool.instance == null || MapManage.instance == null) return;
        Tile tile = GetLeftmostGrassTile(rowY);
        if (tile == null) return;

        GameObject go = ObjectPool.instance.Create(rakePrefab);
        if (go == null) return;
        var rake = go.GetComponent<TileEffect_Rake>();
        if (rake == null)
        {
            ObjectPool.instance.Recycle(go);
            return;
        }
        activeRake = rake;
        rake.EnterTile(tile);
    }

    static Tile GetLeftmostGrassTile(int rowY)
    {
        var map = MapManage.instance;
        if (map?.tiles == null || rowY < 0 || rowY >= map.mapSize.y) return null;
        for (int x = 0; x < map.mapSize.x; x++)
        {
            Tile t = map.tiles[x, rowY];
            if (t != null && (t.tileType & TileType.Grass) != 0)
                return t;
        }
        return map.IfInMapRange(0, rowY) ? map.tiles[0, rowY] : null;
    }

    static Tile FindTileHostingRake(TileEffect_Rake rake)
    {
        var map = MapManage.instance;
        if (map?.tiles == null || rake == null) return null;
        for (int x = 0; x < map.mapSize.x; x++)
        {
            for (int y = 0; y < map.mapSize.y; y++)
            {
                Tile t = map.tiles[x, y];
                if (t?.ObjectsOnTile == null) continue;
                for (int i = 0; i < t.ObjectsOnTile.Count; i++)
                {
                    if (t.ObjectsOnTile[i] == rake)
                        return t;
                }
            }
        }
        return null;
    }
}
