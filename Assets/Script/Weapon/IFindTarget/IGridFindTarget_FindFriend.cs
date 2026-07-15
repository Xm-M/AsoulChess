using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 继承 <see cref="IGridFindTarget"/> 的格子几何与 Scene Gizmo，但索敌层为友方；
/// 默认仅检测地图 X 不小于自身的格子（屏幕右方 / 世界 +X 侧），供治疗弹道等使用。
/// </summary>
[Serializable]
public class IGridFindTarget_FindFriend : IGridFindTarget
{
    const int ColPoolSize = 1000;

    [Tooltip("仅检测 mapPos.x 不小于自身的格子（右方阵列）")]
    public bool onlyMapRight = true;

    [Tooltip("为 true 时只返回 HP 比例最低的一名受伤友方（满血友军不作为目标）")]
    public bool pickLowestHpPercent = true;

    [Tooltip("选治疗目标时是否排除施法者自身")]
    public bool excludeSelf = true;

    public override void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        if (user == null || user.IfDeath || MapManage.instance == null)
            return;

        MapManage map = MapManage.instance;
        if (!GridFindTargetGeometry.TryGetBaseMapPos(user, map, out Vector2Int basePos))
            return;

        int forwardX = GridFindTargetGeometry.GetForwardX(user);
        LayerMask friendLayer = ChessTeamManage.Instance != null
            ? ChessTeamManage.Instance.GetFriendLayer(user.gameObject)
            : LayerMask.GetMask(user.tag);
        Collider2D[] cols = CheckObjectPoolManage.GetColArray(ColPoolSize);
        Vector2 ts = map.tileSize;

        if (relativeCells == null)
        {
            CheckObjectPoolManage.ReleaseColArray(ColPoolSize, cols);
            return;
        }

        Chess best = null;
        float bestHpRatio = float.MaxValue;

        foreach (Vector2Int rel in relativeCells)
        {
            int ax = basePos.x + rel.x * forwardX;
            int ay = basePos.y + rel.y;
            if (onlyMapRight && ax < basePos.x)
                continue;
            if (!GridFindTargetGeometry.IsDetectableCell(ax, ay, map.mapSize))
                continue;

            if (!GridFindTargetGeometry.TryResolveTileAt(ax, ay, map, out Tile tile))
                continue;

            Vector2 center = GridFindTargetGeometry.GetCellOverlapCenter(tile, ts);
            int count = Physics2D.OverlapBoxNonAlloc(center, boxHalfExtents, 0f, cols, friendLayer);
            for (int i = 0; i < count; i++)
            {
                if (cols[i] == null)
                    continue;
                Chess c = cols[i].GetComponent<Chess>();
                if (c == null || c.IfDeath || !IsAlly(user, c))
                    continue;
                if (excludeSelf && c == user)
                    continue;
                if (HealFindTargetUtil.IsAllyAtFullHp(c))
                    continue;

                if (!pickLowestHpPercent)
                {
                    if (!targets.Contains(c))
                        targets.Add(c);
                    continue;
                }

                var pc = c.propertyController;
                float maxHp = pc.GetMaxHp();
                if (maxHp <= 0f)
                    continue;
                float ratio = pc.GetHp() / maxHp;
                if (ratio < bestHpRatio)
                {
                    bestHpRatio = ratio;
                    best = c;
                }
            }
        }

        CheckObjectPoolManage.ReleaseColArray(ColPoolSize, cols);

        if (pickLowestHpPercent && best != null)
            targets.Add(best);
    }

    static bool IsAlly(Chess user, Chess ally)
    {
        if (user.CompareTag(ally.tag))
            return true;
        var tm = ChessTeamManage.Instance;
        return tm != null && tm.GetTeam(tm.player.playerTag).Contains(ally);
    }

#if UNITY_EDITOR
    public override void DrawGizmos(Chess user)
    {
        if (user == null) return;

        MapManage map = MapManage.instance;
        if (map == null && !Application.isPlaying)
            map = UnityEngine.Object.FindObjectOfType<MapManage>();
        if (map == null) return;

        if (!GridFindTargetGeometry.TryGetBaseMapPos(user, map, out Vector2Int basePos))
            return;

        int forwardX = GridFindTargetGeometry.GetForwardX(user);
        Vector2 ts = map.tileSize;

        if (relativeCells == null)
            return;

        Color prev = Gizmos.color;
        Gizmos.color = new Color(0.35f, 1f, 0.45f, 0.95f);

        foreach (Vector2Int rel in relativeCells)
        {
            int ax = basePos.x + rel.x * forwardX;
            int ay = basePos.y + rel.y;
            if (onlyMapRight && ax < basePos.x)
                continue;
            if (!GridFindTargetGeometry.IsDetectableCell(ax, ay, map.mapSize))
                continue;

            if (!GridFindTargetGeometry.TryResolveTileAt(ax, ay, map, out Tile tile))
                continue;

            Vector2 c = GridFindTargetGeometry.GetCellOverlapCenter(tile, ts);
            Vector3 center = new Vector3(c.x, c.y, 0f);
            Vector3 size = new Vector3(boxHalfExtents.x * 2f, boxHalfExtents.y * 2f, 0.05f);
            Gizmos.DrawWireCube(center, size);
        }

        Gizmos.color = prev;
    }
#endif
}
