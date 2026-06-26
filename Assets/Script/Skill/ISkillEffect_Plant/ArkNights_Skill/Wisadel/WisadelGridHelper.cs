using System.Collections.Generic;
using UnityEngine;

/// <summary>维什戴尔格子工具：九宫格索敌、<see cref="IGridFindTarget"/> 攻击范围。</summary>
public static class WisadelGridHelper
{
    const int ColPoolSize = 256;

    public static IGridFindTarget GetGridFindTarget(Chess user)
    {
        if (user?.equipWeapon?.weapon is Weapon_Sample weapon)
            return weapon.findTarget as IGridFindTarget;
        return null;
    }

    /// <summary>收集主人当前朝向下的绝对 mapPos 攻击格（与 <see cref="IGridFindTarget.FindTarget"/> 一致）。</summary>
    public static bool TryCollectAttackCells(Chess user, HashSet<Vector2Int> buffer)
    {
        buffer?.Clear();
        if (buffer == null || user == null || MapManage.instance == null)
            return false;

        var grid = GetGridFindTarget(user);
        if (grid?.relativeCells == null || grid.relativeCells.Count == 0)
            return false;

        if (!GridFindTargetGeometry.TryGetBaseMapPos(user, MapManage.instance, out Vector2Int basePos))
            return false;

        int forwardX = GridFindTargetGeometry.GetForwardX(user);
        Vector2Int mapSize = MapManage.instance.mapSize;

        for (int i = 0; i < grid.relativeCells.Count; i++)
        {
            Vector2Int rel = grid.relativeCells[i];
            int ax = basePos.x + rel.x * forwardX;
            int ay = basePos.y + rel.y;
            if (!GridFindTargetGeometry.IsDetectableCell(ax, ay, mapSize))
                continue;
            buffer.Add(new Vector2Int(ax, ay));
        }

        return buffer.Count > 0;
    }

    public static bool IsTileInAttackCells(Tile tile, HashSet<Vector2Int> attackCells) =>
        tile != null && attackCells != null && attackCells.Contains(tile.mapPos);

    public static Vector2 GetBoxHalfExtents(Chess user)
    {
        var grid = GetGridFindTarget(user);
        return grid != null ? grid.boxHalfExtents : new Vector2(1.4f, 1.1f);
    }

    public static bool TryGetStandOrEstimatedTile(Chess chess, out Tile tile)
    {
        tile = chess?.moveController?.standTile;
        if (tile != null)
            return true;
        if (chess == null || MapManage.instance == null)
            return false;

        MapManage map = MapManage.instance;
        Vector2 p;
        var col = chess.GetComponent<Collider2D>();
        if (col != null)
            p = col.bounds.center;
        else
            p = chess.transform.position;

        Vector2 ts = map.tileSize;
        if (ts.x <= 0f || ts.y <= 0f)
            return false;

        int ix = Mathf.FloorToInt(p.x / ts.x);
        int iy = Mathf.FloorToInt(p.y / ts.y);
        if (!map.IfInMapRange(ix, iy))
            return false;
        tile = map.tiles[ix, iy];
        return tile != null;
    }

    public static bool IsChessInAttackCells(Chess chess, HashSet<Vector2Int> attackCells)
    {
        if (chess == null || attackCells == null)
            return false;
        return TryGetStandOrEstimatedTile(chess, out Tile stand)
            && attackCells.Contains(stand.mapPos);
    }

    public static void CollectNineGridTiles(Tile center, List<Tile> buffer)
    {
        buffer.Clear();
        if (center == null || MapManage.instance == null)
            return;

        buffer.Add(center);
        var neighbors = MapManage.instance.GetEightNeighborTiles(center);
        for (int i = 0; i < neighbors.Count; i++)
        {
            if (neighbors[i] != null)
                buffer.Add(neighbors[i]);
        }
    }

    /// <summary>余震受害者：主目标 + 主目标格九宫格内敌人（OverlapBox，与 <see cref="IGridFindTarget"/> 一致）。</summary>
    public static void CollectAftershockVictims(Chess user, Chess main, List<Tile> tileBuffer, List<Chess> victims)
    {
        victims.Clear();
        if (user == null || main == null || main.IfDeath)
            return;

        victims.Add(main);
        if (!TryGetStandOrEstimatedTile(main, out Tile mainTile))
            return;

        CollectNineGridTiles(mainTile, tileBuffer);
        CollectChessOverlapOnTiles(user, tileBuffer, victims, except: null, clearFirst: false);
    }

    /// <summary>在指定格子上用 OverlapBox 收集敌人（与 <see cref="IGridFindTarget.FindTarget"/> 一致）。</summary>
    public static void CollectChessOverlapOnTiles(
        Chess user,
        IList<Tile> tiles,
        List<Chess> enemies,
        Chess except = null,
        bool clearFirst = true)
    {
        if (clearFirst)
            enemies.Clear();
        if (user == null || tiles == null || enemies == null || MapManage.instance == null)
            return;

        MapManage map = MapManage.instance;
        Vector2 ts = map.tileSize;
        Vector2 half = GetBoxHalfExtents(user);
        LayerMask enemyLayer = ChessTeamManage.Instance.GetEnemyLayer(user.gameObject);
        Collider2D[] cols = CheckObjectPoolManage.GetColArray(ColPoolSize);

        for (int t = 0; t < tiles.Count; t++)
        {
            Tile tile = tiles[t];
            if (tile == null)
                continue;

            Vector2 center = GridFindTargetGeometry.GetCellOverlapCenter(tile, ts);
            int count = Physics2D.OverlapBoxNonAlloc(center, half, 0f, cols, enemyLayer);
            for (int i = 0; i < count; i++)
            {
                if (cols[i] == null)
                    continue;
                Chess chess = cols[i].GetComponent<Chess>();
                if (chess == null || chess.IfDeath || chess == except)
                    continue;
                if (!enemies.Contains(chess))
                    enemies.Add(chess);
            }
        }

        CheckObjectPoolManage.ReleaseColArray(ColPoolSize, cols);
    }

    public static void CollectEnemiesOnTiles(Chess user, IList<Tile> tiles, List<Chess> enemies, Chess except = null) =>
        CollectChessOverlapOnTiles(user, tiles, enemies, except, clearFirst: true);

    public static int ManhattanDistance(Tile a, Tile b)
    {
        if (a == null || b == null)
            return int.MaxValue;
        return Mathf.Abs(a.mapPos.x - b.mapPos.x) + Mathf.Abs(a.mapPos.y - b.mapPos.y);
    }
}
