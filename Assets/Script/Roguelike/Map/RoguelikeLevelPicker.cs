using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 肉鸽战斗节点：按地图层数计算目标难度，从 <see cref="ActMapConfig"/> 关卡池内匹配 <see cref="LevelData.roguelikeDifficulty"/> 后抽取。
/// </summary>
public static class RoguelikeLevelPicker
{
    public static LevelData PickFromPool(
        List<LevelData> pool,
        RoguelikeMapNode node,
        GeneratedRoguelikeMap map,
        ActMapConfig act,
        int runSeed,
        float targetDifficultyBonus = 0f,
        RoguelikeLevelKind requiredKind = RoguelikeLevelKind.None)
    {
        if (pool == null || pool.Count == 0 || node == null || act == null)
            return null;

        pool = FilterByKind(pool, requiredKind);
        if (pool.Count == 0)
            return null;

        float target = ComputeTargetDifficulty(node, map, act) + targetDifficultyBonus;
        float halfRange = Mathf.Max(0.01f, act.roguelikeDifficultyMatchHalfRange);

        var matched = FilterByDifficulty(pool, target, halfRange, requireTaggedDifficulty: true);
        if (matched.Count == 0)
            matched = WidenUntilMatch(pool, target, halfRange);
        if (matched.Count == 0)
            matched = CollectNonNull(pool);

        if (matched.Count == 0)
            return null;

        int idx = Mathf.Abs(runSeed + node.id * 31) % matched.Count;
        return matched[idx];
    }

    /// <summary>层 0 → <see cref="ActMapConfig.roguelikeDifficultyAtStart"/>；Boss 前一层 → <see cref="ActMapConfig.roguelikeDifficultyAtPreBoss"/>。</summary>
    public static float ComputeTargetDifficulty(RoguelikeMapNode node, GeneratedRoguelikeMap map, ActMapConfig act)
    {
        if (node == null || act == null)
            return act != null ? act.roguelikeDifficultyAtStart : 1f;

        var boss = map?.GetBossNode();
        int bossLayer = boss != null ? boss.layer : act.layerCount - 1;
        int preBossLayer = Mathf.Max(1, bossLayer - 1);

        if (bossLayer <= 0 || node.layer <= 0)
            return act.roguelikeDifficultyAtStart;

        if (node.layer >= preBossLayer)
            return act.roguelikeDifficultyAtPreBoss;

        float t = node.layer / (float)preBossLayer;
        return Mathf.Lerp(act.roguelikeDifficultyAtStart, act.roguelikeDifficultyAtPreBoss, t);
    }

    static List<LevelData> FilterByDifficulty(
        List<LevelData> pool,
        float target,
        float halfRange,
        bool requireTaggedDifficulty)
    {
        float min = target - halfRange;
        float max = target + halfRange;
        var list = new List<LevelData>();
        for (int i = 0; i < pool.Count; i++)
        {
            var level = pool[i];
            if (level == null)
                continue;
            if (requireTaggedDifficulty && level.roguelikeDifficulty <= 0f)
                continue;
            if (level.roguelikeDifficulty >= min && level.roguelikeDifficulty <= max)
                list.Add(level);
        }
        return list;
    }

    static List<LevelData> WidenUntilMatch(List<LevelData> pool, float target, float halfRange)
    {
        float range = halfRange;
        for (int attempt = 0; attempt < 8; attempt++)
        {
            range *= 1.5f;
            var list = FilterByDifficulty(pool, target, range, requireTaggedDifficulty: true);
            if (list.Count > 0)
                return list;
        }
        return FilterByDifficulty(pool, target, float.MaxValue * 0.5f, requireTaggedDifficulty: true);
    }

    static List<LevelData> CollectNonNull(List<LevelData> pool)
    {
        var list = new List<LevelData>();
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i] != null)
                list.Add(pool[i]);
        }
        return list;
    }

    static List<LevelData> FilterByKind(List<LevelData> pool, RoguelikeLevelKind requiredKind)
    {
        var list = new List<LevelData>();
        for (int i = 0; i < pool.Count; i++)
        {
            var level = pool[i];
            if (level == null)
                continue;
            if (requiredKind == RoguelikeLevelKind.None || level.roguelikeKind == RoguelikeLevelKind.None)
                list.Add(level);
            else if (level.roguelikeKind == requiredKind)
                list.Add(level);
        }
        return list;
    }
}
