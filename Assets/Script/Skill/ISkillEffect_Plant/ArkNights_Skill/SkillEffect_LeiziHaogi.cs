using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 浩气长存（v1）：左/前/右三份 <see cref="IGridFindTarget"/> 定义范围；
/// 释放时立即三扇区各结算一次物理伤害；落雷特效从中心十字逐格 outward（仅表现，不挡伤害）。
/// </summary>
[Serializable]
public class SkillEffect_LeiziHaogi : ISkillEffect
{
    [LabelText("左扇区")]
    public IGridFindTarget leftGrid = new IGridFindTarget();

    [LabelText("前扇区")]
    public IGridFindTarget frontGrid = new IGridFindTarget();

    [LabelText("右扇区")]
    public IGridFindTarget rightGrid = new IGridFindTarget();

    [LabelText("落雷特效")]
    [Tooltip("留空则尝试 Resources/Effect 下无；建议挂 天雷.prefab")]
    public GameObject strikeFxPrefab;

    [LabelText("落雷间隔")]
    [Min(0f)]
    public float strikeInterval = 0.06f;

    [LabelText("伤害类型")]
    public DamageType damageType = DamageType.Physical;

    static readonly List<Chess> TargetScratch = new List<Chess>(32);
    static readonly List<Vector2Int> OrderedScratch = new List<Vector2Int>(64);
    static readonly HashSet<Vector2Int> UnionScratch = new HashSet<Vector2Int>();

    /// <summary>同波内顺序：前 → 右 → 后 → 左</summary>
    static readonly Vector2Int[] CrossDirs =
    {
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
    };

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || user.IfDeath)
            return;
        // 伤害与特效解耦：释放瞬间按当前扇区结算一次，落雷 FX 仅表现
        ApplySectorDamages(user, config);
        user.StartCoroutine(PlayStrikeFx(user));
    }

    IEnumerator PlayStrikeFx(Chess user)
    {
        BuildCrossOrderedCells(OrderedScratch);
        MapManage map = MapManage.instance;

        for (int i = 0; i < OrderedScratch.Count; i++)
        {
            if (user == null || user.IfDeath)
                yield break;

            SpawnFxAtRelative(user, OrderedScratch[i], map);
            if (strikeInterval > 0f)
                yield return new WaitForSeconds(strikeInterval);
        }
    }

    void BuildCrossOrderedCells(List<Vector2Int> ordered)
    {
        ordered.Clear();
        UnionScratch.Clear();
        AddGridCells(leftGrid);
        AddGridCells(frontGrid);
        AddGridCells(rightGrid);

        bool hasCenter = UnionScratch.Contains(Vector2Int.zero);
        if (hasCenter)
            ordered.Add(Vector2Int.zero);

        int maxRing = 0;
        foreach (var c in UnionScratch)
        {
            if (c.x != 0 && c.y != 0)
                continue;
            maxRing = Mathf.Max(maxRing, Mathf.Abs(c.x) + Mathf.Abs(c.y));
        }

        for (int ring = 1; ring <= maxRing; ring++)
        {
            for (int d = 0; d < CrossDirs.Length; d++)
            {
                Vector2Int cell = CrossDirs[d] * ring;
                if (UnionScratch.Contains(cell))
                    ordered.Add(cell);
            }
        }

        // 并集无 (0,0) 时：从 Manhattan 最近格起（仍只播十字格）
        if (!hasCenter && ordered.Count == 0)
        {
            Vector2Int best = default;
            int bestDist = int.MaxValue;
            bool found = false;
            foreach (var c in UnionScratch)
            {
                if (c.x != 0 && c.y != 0)
                    continue;
                int dist = Mathf.Abs(c.x) + Mathf.Abs(c.y);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = c;
                    found = true;
                }
            }
            if (found)
                ordered.Add(best);
        }
    }

    void AddGridCells(IGridFindTarget grid)
    {
        if (grid?.relativeCells == null)
            return;
        for (int i = 0; i < grid.relativeCells.Count; i++)
            UnionScratch.Add(grid.relativeCells[i]);
    }

    void SpawnFxAtRelative(Chess user, Vector2Int rel, MapManage map)
    {
        if (strikeFxPrefab == null || ObjectPool.instance == null || map == null)
            return;
        if (!GridFindTargetGeometry.TryGetBaseMapPos(user, map, out Vector2Int basePos))
            return;

        int forwardX = GridFindTargetGeometry.GetForwardX(user);
        int ax = basePos.x + rel.x * forwardX;
        int ay = basePos.y + rel.y;
        if (!GridFindTargetGeometry.IsDetectableCell(ax, ay, map.mapSize))
            return;
        if (!GridFindTargetGeometry.TryResolveTileAt(ax, ay, map, out Tile tile) || tile == null)
            return;

        GameObject fx = ObjectPool.instance.Create(strikeFxPrefab);
        if (fx == null)
            return;
        Vector2 center = GridFindTargetGeometry.GetCellOverlapCenter(tile, map.tileSize);
        fx.transform.position = new Vector3(center.x, center.y, 0f);
    }

    void ApplySectorDamages(Chess user, SkillConfig config)
    {
        float mul = 3f;
        if (config?.baseDamage != null && config.baseDamage.Count > 0)
            mul = config.baseDamage[0];
        float damage = user.propertyController.GetAttack() * mul;

        ApplyOneSector(user, leftGrid, damage);
        ApplyOneSector(user, frontGrid, damage);
        ApplyOneSector(user, rightGrid, damage);
    }

    void ApplyOneSector(Chess user, IGridFindTarget grid, float damage)
    {
        if (grid == null || damage <= 0f)
            return;
        TargetScratch.Clear();
        grid.FindTarget(user, TargetScratch);
        for (int i = 0; i < TargetScratch.Count; i++)
        {
            Chess enemy = TargetScratch[i];
            if (enemy == null || enemy.IfDeath || enemy.propertyController == null)
                continue;
            var dm = new DamageMessege(user, enemy, damage, damageType, ElementType.None);
            enemy.propertyController.GetDamage(dm);
        }
    }
}
