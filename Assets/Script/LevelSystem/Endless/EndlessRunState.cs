using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 无尽/生存模式轮次运行时状态：本轮出场池、轮次索引、累计波次与稀有度衰减。
/// </summary>
public class EndlessRunState
{
    public int selectionIndex = 1;
    public int totalWavesCleared;
    public List<PropertyCreator> segmentPool = new List<PropertyCreator>();

    readonly Dictionary<PropertyCreator, int> rarityUseCount = new Dictionary<PropertyCreator, int>();
    EnterMapPlugin_EndlessSpawnConfig config;

    public void BindConfig(EnterMapPlugin_EndlessSpawnConfig spawnConfig)
    {
        config = spawnConfig;
    }

    public void RebuildSegmentPool(LevelData levelData, int roundIndex)
    {
        segmentPool.Clear();
        rarityUseCount.Clear();
        if (levelData?.zombieList == null || levelData.zombieList.Count == 0)
            return;

        int initial = config != null ? config.initialPoolSize : 4;
        int growth = config != null ? config.poolGrowthPerRound : 1;
        int targetCount = Mathf.Min(initial + (roundIndex - 1) * growth, levelData.zombieList.Count);
        targetCount = Mathf.Max(1, targetCount);

        for (int i = 0; i < levelData.zombieList.Count && segmentPool.Count < targetCount; i++)
        {
            var creator = levelData.zombieList[i];
            if (creator != null)
                segmentPool.Add(creator);
        }
    }

    public void RestoreSegmentPoolFromIds(LevelData levelData, IList<string> ids)
    {
        segmentPool.Clear();
        rarityUseCount.Clear();
        if (levelData?.zombieList == null || ids == null || ids.Count == 0)
            return;

        var lookup = new Dictionary<string, PropertyCreator>();
        foreach (var z in levelData.zombieList)
        {
            if (z != null && !string.IsNullOrEmpty(z.chessName))
                lookup[z.chessName] = z;
        }

        foreach (var id in ids)
        {
            if (!string.IsNullOrEmpty(id) && lookup.TryGetValue(id, out var creator))
                segmentPool.Add(creator);
        }
    }

    public List<string> GetSegmentPoolIds()
    {
        var ids = new List<string>();
        foreach (var creator in segmentPool)
        {
            if (creator != null && !string.IsNullOrEmpty(creator.chessName))
                ids.Add(creator.chessName);
        }
        return ids;
    }

    public int GetEffectiveRarity(PropertyCreator creator)
    {
        if (creator?.baseProperty == null)
            return 1;
        int baseRarity = creator.baseProperty.rarity;
        int decay = config != null ? config.rarityDecayPerUse : 0;
        if (decay <= 0)
            return Mathf.Max(1, baseRarity);

        rarityUseCount.TryGetValue(creator, out int uses);
        return Mathf.Max(1, baseRarity - uses * decay);
    }

    public void RecordRarityUse(PropertyCreator creator)
    {
        if (creator == null)
            return;
        rarityUseCount.TryGetValue(creator, out int uses);
        rarityUseCount[creator] = uses + 1;
    }

    public void ResetRarityUseCounts()
    {
        rarityUseCount.Clear();
    }
}
