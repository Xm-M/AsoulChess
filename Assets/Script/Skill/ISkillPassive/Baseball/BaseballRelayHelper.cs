using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 投手喂球 / 挥棒手接球：友方范围扫描、角色标签、敌人门控。
/// 友方检测独立于 <see cref="StraightFindTarget"/>（只扫敌层）。
/// </summary>
public static class BaseballRelayHelper
{
    const int ColPoolSize = 256;
    static readonly List<Chess> EnemyBuffer = new List<Chess>();

    public static bool IsPitcher(Chess chess) => HasPlantTag(chess, BaseballRelayKeys.PitcherRoleTag);

    public static bool IsBatter(Chess chess) => HasPlantTag(chess, BaseballRelayKeys.BatterRoleTag);

    static bool HasPlantTag(Chess chess, string tag)
    {
        var tags = chess?.propertyController?.creator?.plantTags;
        return tags != null && tags.Contains(tag);
    }

    /// <summary>投手自身敌人索敌范围内是否有敌人（B1 门控）。</summary>
    public static bool HasEnemyInPitcherRange(Chess pitcher)
    {
        if (pitcher?.equipWeapon?.weapon == null)
            return false;

        EnemyBuffer.Clear();
        IFindTarget enemyFind = ResolveEnemyFindTarget(pitcher);
        enemyFind?.FindTarget(pitcher, EnemyBuffer);
        for (int i = 0; i < EnemyBuffer.Count; i++)
        {
            Chess c = EnemyBuffer[i];
            if (c != null && !c.IfDeath)
                return true;
        }

        return false;
    }

    /// <summary>在投手攻击范围（友方通道）内找最近的前向挥棒手。</summary>
    public static Chess TryFindBatterInRelayRange(Chess pitcher)
    {
        if (pitcher == null || pitcher.IfDeath || MapManage.instance == null)
            return null;

        var weapon = pitcher.equipWeapon?.weapon as Weapon_Sample;
        IFindTarget source = ResolveGeometryFindTarget(pitcher);
        if (source is IGridFindTarget grid)
            return TryFindBatterGrid(pitcher, grid);

        float range = pitcher.propertyController != null
            ? pitcher.propertyController.GetAttackRange()
            : 0f;
        if (range <= 0f)
            return null;

        return TryFindBatterRay(pitcher, range);
    }

    static IFindTarget ResolveEnemyFindTarget(Chess pitcher)
    {
        var weapon = pitcher.equipWeapon?.weapon as Weapon_Sample;
        IFindTarget find = weapon?.findTarget;
        if (find is FindTarget_BaseballPitcherRelay relay)
            return relay.enemyFindTarget;
        return find;
    }

    static IFindTarget ResolveGeometryFindTarget(Chess pitcher)
    {
        IFindTarget find = ResolveEnemyFindTarget(pitcher);
        return find ?? (pitcher.equipWeapon?.weapon as Weapon_Sample)?.findTarget;
    }

    static Chess TryFindBatterGrid(Chess pitcher, IGridFindTarget grid)
    {
        MapManage map = MapManage.instance;
        if (!GridFindTargetGeometry.TryGetBaseMapPos(pitcher, map, out Vector2Int basePos))
            return null;

        int forwardX = GridFindTargetGeometry.GetForwardX(pitcher);
        LayerMask friendLayer = ChessTeamManage.Instance != null
            ? ChessTeamManage.Instance.GetFriendLayer(pitcher.gameObject)
            : LayerMask.GetMask(pitcher.tag);
        Collider2D[] cols = CheckObjectPoolManage.GetColArray(ColPoolSize);
        Vector2 ts = map.tileSize;

        Chess best = null;
        int bestForward = int.MaxValue;

        if (grid.relativeCells == null)
        {
            CheckObjectPoolManage.ReleaseColArray(ColPoolSize, cols);
            return null;
        }

        for (int i = 0; i < grid.relativeCells.Count; i++)
        {
            Vector2Int rel = grid.relativeCells[i];
            if (rel.x <= 0)
                continue;

            int ax = basePos.x + rel.x * forwardX;
            int ay = basePos.y + rel.y;
            if (ay != basePos.y)
                continue;
            if (!GridFindTargetGeometry.IsDetectableCell(ax, ay, map.mapSize))
                continue;

            if (!GridFindTargetGeometry.TryResolveTileAt(ax, ay, map, out Tile tile) || tile == null)
                continue;

            Vector2 center = GridFindTargetGeometry.GetCellOverlapCenter(tile, ts);
            int count = Physics2D.OverlapBoxNonAlloc(center, grid.boxHalfExtents, 0f, cols, friendLayer);
            for (int j = 0; j < count; j++)
            {
                if (cols[j] == null)
                    continue;
                Chess c = cols[j].GetComponent<Chess>();
                if (c == null || c.IfDeath || c == pitcher || !IsBatter(c))
                    continue;
                if (!IsAlly(pitcher, c))
                    continue;
                if (!IsSameRelayRow(pitcher, c))
                    continue;

                int forwardDist = Mathf.Abs(ax - basePos.x);
                if (forwardDist < bestForward)
                {
                    bestForward = forwardDist;
                    best = c;
                }
            }
        }

        CheckObjectPoolManage.ReleaseColArray(ColPoolSize, cols);
        return best;
    }

    static Chess TryFindBatterRay(Chess pitcher, float range)
    {
        LayerMask friendLayer = ChessTeamManage.Instance != null
            ? ChessTeamManage.Instance.GetFriendLayer(pitcher.gameObject)
            : LayerMask.GetMask(pitcher.tag);

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            pitcher.transform.position,
            pitcher.transform.right,
            range,
            friendLayer);

        Chess best = null;
        float bestDist = float.MaxValue;
        for (int i = 0; i < hits.Length; i++)
        {
            Chess c = hits[i].collider != null ? hits[i].collider.GetComponent<Chess>() : null;
            if (c == null || c.IfDeath || c == pitcher || !IsBatter(c))
                continue;
            if (!IsAlly(pitcher, c))
                continue;
            if (!IsSameRelayRow(pitcher, c))
                continue;
            if (hits[i].distance < bestDist)
            {
                bestDist = hits[i].distance;
                best = c;
            }
        }

        return best;
    }

    static bool IsAlly(Chess a, Chess b)
    {
        if (a == null || b == null)
            return false;
        if (a.CompareTag(b.tag))
            return true;
        var tm = ChessTeamManage.Instance;
        return tm != null && tm.GetTeam(a.tag).Contains(b);
    }

    /// <summary>投手与挥棒手须在同一地图行（PVZ 分行）。</summary>
    public static bool TryGetRelayRow(Chess chess, out int rowY)
    {
        rowY = 0;
        if (chess == null)
            return false;

        if (chess.moveController?.standTile != null)
        {
            rowY = chess.moveController.standTile.mapPos.y;
            return true;
        }

        MapManage map = MapManage.instance;
        if (map != null && GridFindTargetGeometry.TryGetBaseMapPos(chess, map, out Vector2Int pos))
        {
            rowY = pos.y;
            return true;
        }

        return false;
    }

    public static bool IsSameRelayRow(Chess a, Chess b)
    {
        if (!TryGetRelayRow(a, out int rowA) || !TryGetRelayRow(b, out int rowB))
            return false;
        return rowA == rowB;
    }

    public static void DeliverPitch(Chess batter, Chess pitcher)
    {
        if (batter == null || pitcher == null || batter.IfDeath || pitcher.IfDeath)
            return;
        if (!IsBatter(batter) || !IsPitcher(pitcher))
            return;
        if (!IsAlly(pitcher, batter))
            return;
        if (!IsSameRelayRow(pitcher, batter))
            return;

        BaseballBatterReceiveRegistry.TryReceive(batter, pitcher);
    }
}
