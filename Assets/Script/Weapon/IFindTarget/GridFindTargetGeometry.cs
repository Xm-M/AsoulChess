using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 与 <see cref="IGridFindTarget"/> 共用的格子几何（世界坐标、朝向镜像、可检测范围），
/// 供索敌与 <see cref="IBulletMove_Bundling"/> 等复用。
/// </summary>
public static class GridFindTargetGeometry
{
    /// <summary>逻辑列：战场左侧 <see cref="MapManage_PVZ.roomTile"/>（小推车列），与主网格 <c>tiles[0,y]</c> 相邻。</summary>
    public const int LawnMowerColumnMapX = -1;

    public static int GetForwardX(Chess user)
    {
        float rx = user.transform.right.x;
        if (Mathf.Approximately(rx, 0f))
            return 1;
        return rx > 0f ? 1 : -1;
    }

    /// <summary>
    /// 有 standTile 用其 mapPos；否则用世界坐标按 <see cref="MapManage.tileSize"/> 估格并钳到地图内。
    /// </summary>
    public static bool TryGetBaseMapPos(Chess user, MapManage map, out Vector2Int outPos)
    {
        outPos = default;
        if (user.moveController != null && user.moveController.standTile != null)
        {
            outPos = user.moveController.standTile.mapPos;
            return true;
        }

        Vector2 p = user.transform.position;
        Vector2 ts = map.tileSize;
        if (ts.x <= 0f || ts.y <= 0f)
            return false;

        int ix = Mathf.FloorToInt(p.x / ts.x);
        int iy = Mathf.FloorToInt(p.y / ts.y);
        ix = Mathf.Clamp(ix, 0, Mathf.Max(0, map.mapSize.x - 1));
        iy = Mathf.Clamp(iy, 0, Mathf.Max(0, map.mapSize.y - 1));
        outPos = new Vector2Int(ix, iy);
        return true;
    }

    /// <summary>
    /// 小推车列（<see cref="LawnMowerColumnMapX"/>）或主战场格是否可参与格子索敌/铺冰检测。
    /// Y 全高有效；主网格 X 排除最右一列（下标 mapSize.x - 1）。
    /// </summary>
    public static bool IsDetectableCell(int x, int y, Vector2Int mapSize)
    {
        if (mapSize.y <= 0)
            return false;
        if (y < 0 || y >= mapSize.y)
            return false;
        if (x == LawnMowerColumnMapX)
            return TryGetRoomTile(y, out _);
        if (mapSize.x < 2)
            return false;
        if (x < 0 || x > mapSize.x - 2)
            return false;
        return true;
    }

    /// <summary>按行取下推车列 <see cref="MapManage_PVZ.roomTile"/>。</summary>
    public static bool TryGetRoomTile(int rowY, out Tile tile)
    {
        tile = null;
        var pvz = MapManage.instance as MapManage_PVZ;
        if (pvz?.roomTile == null || rowY < 0 || rowY >= pvz.roomTile.Count)
            return false;
        tile = pvz.roomTile[rowY];
        return tile != null;
    }

    /// <summary>主网格或推车列解析为 <see cref="Tile"/>。</summary>
    public static bool TryResolveTileAt(int ax, int ay, MapManage map, out Tile tile)
    {
        tile = null;
        if (map == null)
            return false;
        Vector2Int mapSize = map.mapSize;
        if (ay < 0 || ay >= mapSize.y)
            return false;
        if (ax == LawnMowerColumnMapX)
            return TryGetRoomTile(ay, out tile);
        if (ax < 0 || ax >= mapSize.x)
            return false;
        tile = map.tiles[ax, ay];
        return tile != null;
    }

    /// <summary>冰块字典键：推车列用 <c>(-1, roomTile 行索引)</c>，否则 <see cref="Tile.mapPos"/>。</summary>
    public static Vector2Int TileToIceKey(Tile tile)
    {
        if (tile == null)
            return new Vector2Int(int.MinValue, 0);
        var pvz = MapManage.instance as MapManage_PVZ;
        if (pvz?.roomTile != null)
        {
            int idx = pvz.roomTile.IndexOf(tile);
            if (idx >= 0)
                return new Vector2Int(LawnMowerColumnMapX, idx);
        }
        return tile.mapPos;
    }

    /// <summary>
    /// 单格 OverlapBox 的世界中心（与 <see cref="IGridFindTarget"/> 一致）。
    /// </summary>
    public static Vector2 GetCellOverlapCenter(Tile tile, Vector2 tileSize)
    {
        if (tile == null) return default;

        var sr = tile.GetComponentInChildren<SpriteRenderer>(true);
        if (sr != null)
            return sr.bounds.center;

        var col = tile.GetComponentInChildren<Collider2D>(true);
        if (col != null)
            return col.bounds.center;

        return (Vector2)tile.transform.position + new Vector2(tileSize.x * 0.5f, tileSize.y * 0.5f);
    }

    /// <summary>
    /// 集束弹道用：在 <paramref name="relativeCells"/> 中取最小 relative.x 为「近沿」、最大 relative.x 为「远沿」，
    /// 按同一 <c>rel.y</c> 配对近/远格中心；无共同 rel.y 时退回近/远重心。
    /// </summary>
    public static bool TryGetBundlingLaneEndpoints(
        Chess user,
        List<Vector2Int> relativeCells,
        int laneIndex,
        out Vector2 nearWorld,
        out Vector2 farWorld,
        out int laneCount)
    {
        nearWorld = farWorld = default;
        laneCount = 1;

        if (user == null || relativeCells == null || relativeCells.Count == 0)
            return false;

        MapManage map = MapManage.instance;
        if (map == null || !TryGetBaseMapPos(user, map, out Vector2Int basePos))
            return false;

        int forwardX = GetForwardX(user);
        Vector2 ts = map.tileSize;

        int minX = int.MaxValue, maxX = int.MinValue;
        for (int i = 0; i < relativeCells.Count; i++)
        {
            Vector2Int r = relativeCells[i];
            if (r.x < minX) minX = r.x;
            if (r.x > maxX) maxX = r.x;
        }

        var nearByY = new Dictionary<int, Vector2>();
        var farByY = new Dictionary<int, Vector2>();

        for (int i = 0; i < relativeCells.Count; i++)
        {
            Vector2Int rel = relativeCells[i];
            if (rel.x != minX && rel.x != maxX)
                continue;

            int ax = basePos.x + rel.x * forwardX;
            int ay = basePos.y + rel.y;
            if (!IsDetectableCell(ax, ay, map.mapSize))
                continue;

            if (!TryResolveTileAt(ax, ay, map, out Tile tile))
                continue;

            Vector2 c = GetCellOverlapCenter(tile, ts);
            if (rel.x == minX)
                nearByY[rel.y] = c;
            if (rel.x == maxX)
                farByY[rel.y] = c;
        }

        if (nearByY.Count == 0 || farByY.Count == 0)
            return false;

        if (minX == maxX)
        {
            Vector2 only = default;
            foreach (var kv in nearByY)
            {
                only = kv.Value;
                break;
            }
            nearWorld = only;
            farWorld = only + (Vector2)user.transform.right * (ts.x * 2f);
            laneCount = 1;
            return true;
        }

        var commonYs = new List<int>();
        foreach (int y in nearByY.Keys)
        {
            if (farByY.ContainsKey(y))
                commonYs.Add(y);
        }

        commonYs.Sort();

        if (commonYs.Count == 0)
        {
            nearWorld = AverageValues(nearByY);
            farWorld = AverageValues(farByY);
            laneCount = 1;
            if (Vector2.Distance(nearWorld, farWorld) < 1e-3f)
                farWorld = nearWorld + (Vector2)user.transform.right * (ts.x * 2f);
            return true;
        }

        laneCount = commonYs.Count;
        int idx = Mathf.Abs(laneIndex) % laneCount;
        int yk = commonYs[idx];
        nearWorld = nearByY[yk];
        farWorld = farByY[yk];
        if (Vector2.Distance(nearWorld, farWorld) < 1e-3f)
            farWorld = nearWorld + (Vector2)user.transform.right * (ts.x * 2f);
        return true;
    }

    /// <summary>
    /// 近沿宽边在世界 XY 上简化为竖线段：<c>x</c> 取近沿各格心世界 <c>x</c> 的平均，
    /// <c>y</c> 在格心世界 <c>y</c> 的最小～最大间均匀随机，
    /// 得到点 <c>(edgeX, y)</c> 供「该点 → 发射点」作为初向（由调用方算 <c>start - world</c>）。
    /// </summary>
    public static bool TryGetRandomPointOnNearWidth(Chess user, List<Vector2Int> relativeCells, out Vector2 world)
    {
        world = default;
        if (user == null || relativeCells == null || relativeCells.Count == 0)
            return false;

        MapManage map = MapManage.instance;
        if (map == null || !TryGetBaseMapPos(user, map, out Vector2Int basePos))
            return false;

        int forwardX = GetForwardX(user);
        Vector2 ts = map.tileSize;

        int minX = int.MaxValue;
        for (int i = 0; i < relativeCells.Count; i++)
        {
            if (relativeCells[i].x < minX)
                minX = relativeCells[i].x;
        }

        var centers = new List<Vector2>();
        for (int i = 0; i < relativeCells.Count; i++)
        {
            Vector2Int rel = relativeCells[i];
            if (rel.x != minX)
                continue;

            int ax = basePos.x + rel.x * forwardX;
            int ay = basePos.y + rel.y;
            if (!IsDetectableCell(ax, ay, map.mapSize))
                continue;

            if (!TryResolveTileAt(ax, ay, map, out Tile tile))
                continue;

            centers.Add(GetCellOverlapCenter(tile, ts));
        }

        if (centers.Count == 0)
            return false;

        float sumX = 0f;
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;
        for (int i = 0; i < centers.Count; i++)
        {
            Vector2 c = centers[i];
            sumX += c.x;
            if (c.y < minY) minY = c.y;
            if (c.y > maxY) maxY = c.y;
        }

        float edgeX = sumX / centers.Count;
        float y = Mathf.Approximately(minY, maxY) ? minY : Random.Range(minY, maxY);
        world = new Vector2(edgeX, y);
        return true;
    }

    static Vector2 AverageValues(Dictionary<int, Vector2> map)
    {
        Vector2 s = Vector2.zero;
        int n = 0;
        foreach (var kv in map)
        {
            s += kv.Value;
            n++;
        }
        return n > 0 ? s / n : Vector2.zero;
    }
}
