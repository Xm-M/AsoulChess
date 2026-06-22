using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 肉鸽 Run 经济规则：按 <see cref="RoguelikeLevelKind"/> 配置战斗金币、植物三选一等奖励。
/// 挂在 <see cref="RunMapConfig.economyConfig"/>；未配置时使用 <see cref="CreateRuntimeDefaults"/>。
/// </summary>
[CreateAssetMenu(fileName = "RoguelikeEconomyConfig", menuName = "Roguelike/Economy Config")]
public class RoguelikeEconomyConfig : ScriptableObject
{
    [LabelText("金币规则")]
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<RoguelikeGoldRule> goldRules = new List<RoguelikeGoldRule>();

    [FoldoutGroup("植物三选一")]
    [LabelText("price 分档")]
    public RoguelikePlantPriceBands plantPriceBands = new RoguelikePlantPriceBands();

    [FoldoutGroup("植物三选一")]
    [LabelText("候选池筛选")]
    public RoguelikePlantPoolFilter plantPoolFilter = new RoguelikePlantPoolFilter();

    [FoldoutGroup("植物三选一")]
    [LabelText("PlantPick 规则")]
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<RoguelikePlantPickRule> plantPickRules = new List<RoguelikePlantPickRule>();

    /// <summary>按关卡类型掷金币；无匹配规则或 disabled 时返回 0。</summary>
    public int RollGold(RoguelikeLevelKind kind, RoguelikeRunState state, LevelData level)
    {
        if (state == null || kind == RoguelikeLevelKind.None)
            return 0;

        var rule = FindGoldRule(kind);
        if (rule == null || !rule.enabled)
            return 0;

        int min = Mathf.Min(rule.goldMin, rule.goldMax);
        int max = Mathf.Max(rule.goldMin, rule.goldMax);
        if (max <= 0)
            return 0;
        if (min >= max)
            return min;

        var rng = CreateRewardRng(state, level, kind, salt: 97);
        return rng.Next(min, max + 1);
    }

    /// <summary>该关卡类型是否应生成 PlantPick 搜刮条目。</summary>
    public bool ShouldGrantPlantPick(RoguelikeLevelKind kind)
    {
        if (kind == RoguelikeLevelKind.None)
            return false;

        var rule = FindPlantPickRule(kind);
        return rule != null && rule.enabled && rule.grantPlantPick && rule.optionCount > 0;
    }

    /// <summary>
    /// 杀戮尖塔式两步抽选：每格先掷 price 档位，再从候选池均匀随机 1 张 plant。
    /// 返回 creator.chessName 列表；无规则或池子为空时返回空列表。
    /// </summary>
    public List<string> RollPlantPickOptions(
        RoguelikeLevelKind kind,
        RoguelikeRunState state,
        LevelData level)
    {
        var result = new List<string>();
        if (state == null || kind == RoguelikeLevelKind.None)
            return result;

        var rule = FindPlantPickRule(kind);
        if (rule == null || !rule.enabled || !rule.grantPlantPick || rule.optionCount <= 0)
            return result;

        var pool = BuildEligiblePlantPool(state);
        if (pool.Count == 0)
        {
            Debug.LogWarning($"[RoguelikeEconomyConfig] PlantPick 候选池为空（kind={kind}）");
            return result;
        }

        var rng = CreateRewardRng(state, level, kind, salt: 0x504C414E);
        var bands = plantPriceBands ?? new RoguelikePlantPriceBands();
        var weights = rule.tierWeights ?? new RoguelikePlantTierWeights();
        bool dedupeOffer = plantPoolFilter == null || plantPoolFilter.excludeDuplicatesInOffer;
        var working = new List<PropertyCreator>(pool);

        int count = Mathf.Min(rule.optionCount, working.Count);
        for (int slot = 0; slot < count; slot++)
        {
            var tier = weights.RollTier(rng);
            var tierPool = FilterByPriceTier(working, bands, tier);
            if (tierPool.Count == 0)
                tierPool = working;

            int idx = rng.Next(tierPool.Count);
            var pick = tierPool[idx];
            if (pick == null || string.IsNullOrEmpty(pick.chessName))
                continue;

            result.Add(pick.chessName);
            if (dedupeOffer)
                working.Remove(pick);
            if (working.Count == 0)
                break;
        }

        return result;
    }

    static System.Random CreateRewardRng(
        RoguelikeRunState state,
        LevelData level,
        RoguelikeLevelKind kind,
        int salt)
    {
        int seed = state.runSeed
                   ^ state.currentActIndex * 10007
                   ^ (state.clearedNodeIds?.Count ?? 0) * 7919
                   ^ (level != null ? level.name.GetHashCode() : 0)
                   ^ (int)kind * salt;
        return new System.Random(seed);
    }

    List<PropertyCreator> BuildEligiblePlantPool(RoguelikeRunState state)
    {
        var result = new List<PropertyCreator>();
        var filter = plantPoolFilter ?? new RoguelikePlantPoolFilter();
        var source = GetPlantPoolSource(filter);
        if (source == null || source.Count == 0)
            return result;

        var owned = state?.ownedPlantCreatorIds;
        for (int i = 0; i < source.Count; i++)
        {
            var creator = source[i];
            if (!IsEligiblePlantCreator(creator, filter, owned))
                continue;
            if (ContainsCreator(result, creator))
                continue;
            result.Add(creator);
        }

        return result;
    }

    static List<PropertyCreator> GetPlantPoolSource(RoguelikePlantPoolFilter filter)
    {
        if (filter.whitelistPool != null && filter.whitelistPool.Count > 0)
            return filter.whitelistPool;

        return GameManage.instance != null ? GameManage.instance.allChess : null;
    }

    static bool IsEligiblePlantCreator(
        PropertyCreator creator,
        RoguelikePlantPoolFilter filter,
        List<string> ownedIds)
    {
        if (creator == null || string.IsNullOrEmpty(creator.chessName))
            return false;

        if (!IsPlantRewardCandidate(creator))
            return false;

        if (filter.excludeLevelUpPlants && IsLevelUpPlant(creator))
            return false;

        if (filter.blacklistCreatorIds != null && filter.blacklistCreatorIds.Contains(creator.chessName))
            return false;

        if (filter.excludeOwnedInRun
            && ownedIds != null
            && ownedIds.Contains(creator.chessName))
            return false;

        return true;
    }

    /// <summary>有种植逻辑或卡牌预制体的 creator 视为 plant 奖励候选。</summary>
    static bool IsPlantRewardCandidate(PropertyCreator creator)
    {
        if (creator.plantFunction != null)
            return true;
        if (creator.PlantCardPre != null || creator.PlantEntrepotCardPre != null)
            return true;
        return false;
    }

    static bool IsLevelUpPlant(PropertyCreator creator) =>
        creator.plantIfCanBuyCard is LevelUp_Limit;

    static bool ContainsCreator(List<PropertyCreator> list, PropertyCreator creator)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == creator)
                return true;
        }
        return false;
    }

    static List<PropertyCreator> FilterByPriceTier(
        List<PropertyCreator> pool,
        RoguelikePlantPriceBands bands,
        RoguelikePlantPriceTier tier)
    {
        var list = new List<PropertyCreator>();
        for (int i = 0; i < pool.Count; i++)
        {
            var creator = pool[i];
            if (creator?.baseProperty == null)
                continue;
            if (bands.GetTier(creator.baseProperty.price) == tier)
                list.Add(creator);
        }
        return list;
    }

    RoguelikeGoldRule FindGoldRule(RoguelikeLevelKind kind)
    {
        if (goldRules == null || goldRules.Count == 0)
            return null;

        for (int i = 0; i < goldRules.Count; i++)
        {
            if (goldRules[i].kind == kind)
                return goldRules[i];
        }
        return null;
    }

    RoguelikePlantPickRule FindPlantPickRule(RoguelikeLevelKind kind)
    {
        if (plantPickRules == null || plantPickRules.Count == 0)
            return null;

        for (int i = 0; i < plantPickRules.Count; i++)
        {
            if (plantPickRules[i].kind == kind)
                return plantPickRules[i];
        }
        return null;
    }

    [Button("重置为尖塔参考默认值")]
    public void ResetToStsDefaults()
    {
        goldRules = new List<RoguelikeGoldRule>
        {
            new RoguelikeGoldRule { kind = RoguelikeLevelKind.Normal, goldMin = 10, goldMax = 20 },
            new RoguelikeGoldRule { kind = RoguelikeLevelKind.Elite, goldMin = 25, goldMax = 35 },
            new RoguelikeGoldRule { kind = RoguelikeLevelKind.Boss, goldMin = 95, goldMax = 105 },
            new RoguelikeGoldRule { kind = RoguelikeLevelKind.Event, goldMin = 15, goldMax = 30 },
        };

        plantPriceBands = new RoguelikePlantPriceBands();
        plantPoolFilter = new RoguelikePlantPoolFilter();
        plantPickRules = CreateDefaultPlantPickRules();
    }

    static List<RoguelikePlantPickRule> CreateDefaultPlantPickRules()
    {
        return new List<RoguelikePlantPickRule>
        {
            new RoguelikePlantPickRule
            {
                kind = RoguelikeLevelKind.Normal,
                grantPlantPick = false,
                optionCount = 3,
                tierWeights = RoguelikePlantTierWeights.StsNormal(),
            },
            new RoguelikePlantPickRule
            {
                kind = RoguelikeLevelKind.Elite,
                grantPlantPick = true,
                optionCount = 3,
                tierWeights = RoguelikePlantTierWeights.StsElite(),
            },
            new RoguelikePlantPickRule
            {
                kind = RoguelikeLevelKind.Boss,
                grantPlantPick = true,
                optionCount = 3,
                tierWeights = RoguelikePlantTierWeights.StsBoss(),
            },
            new RoguelikePlantPickRule
            {
                kind = RoguelikeLevelKind.Event,
                grantPlantPick = false,
                optionCount = 3,
                tierWeights = RoguelikePlantTierWeights.StsNormal(),
            },
        };
    }

    /// <summary>未挂 economyConfig 时的内存默认（尖塔参考值）。</summary>
    public static RoguelikeEconomyConfig CreateRuntimeDefaults()
    {
        var cfg = CreateInstance<RoguelikeEconomyConfig>();
        cfg.ResetToStsDefaults();
        return cfg;
    }

    void OnEnable()
    {
        if (goldRules == null || goldRules.Count == 0)
            ResetToStsDefaults();
        else if (plantPickRules == null || plantPickRules.Count == 0)
            plantPickRules = CreateDefaultPlantPickRules();
    }
}

[Serializable]
public class RoguelikeGoldRule
{
    [LabelText("关卡类型")]
    public RoguelikeLevelKind kind = RoguelikeLevelKind.Normal;

    [LabelText("最少金币")]
    [MinValue(0)]
    public int goldMin = 10;

    [LabelText("最多金币")]
    [MinValue(0)]
    public int goldMax = 20;

    [LabelText("启用")]
    public bool enabled = true;
}

/// <summary>用 <see cref="Property.baseProperty.price"/> 映射杀戮尖塔 Common / Uncommon / Rare。</summary>
public enum RoguelikePlantPriceTier
{
    Low = 0,
    Mid = 1,
    High = 2,
}

[Serializable]
public class RoguelikePlantPriceBands
{
    [LabelText("低档 price 上限（含）")]
    [MinValue(0)]
    public int lowMaxPrice = 75;

    [LabelText("中档 price 上限（含）")]
    [MinValue(0)]
    public int midMaxPrice = 125;

    public RoguelikePlantPriceTier GetTier(int price)
    {
        if (price <= lowMaxPrice)
            return RoguelikePlantPriceTier.Low;
        if (price <= midMaxPrice)
            return RoguelikePlantPriceTier.Mid;
        return RoguelikePlantPriceTier.High;
    }
}

[Serializable]
public class RoguelikePlantTierWeights
{
    [LabelText("低档权重")]
    [MinValue(0)]
    public int weightLow = 60;

    [LabelText("中档权重")]
    [MinValue(0)]
    public int weightMid = 37;

    [LabelText("高档权重")]
    [MinValue(0)]
    public int weightHigh = 3;

    public RoguelikePlantPriceTier RollTier(System.Random rng)
    {
        int total = weightLow + weightMid + weightHigh;
        if (total <= 0)
            return RoguelikePlantPriceTier.Low;

        int roll = rng.Next(total);
        if (roll < weightLow)
            return RoguelikePlantPriceTier.Low;
        roll -= weightLow;
        if (roll < weightMid)
            return RoguelikePlantPriceTier.Mid;
        return RoguelikePlantPriceTier.High;
    }

    public static RoguelikePlantTierWeights StsNormal() =>
        new RoguelikePlantTierWeights { weightLow = 60, weightMid = 37, weightHigh = 3 };

    public static RoguelikePlantTierWeights StsElite() =>
        new RoguelikePlantTierWeights { weightLow = 45, weightMid = 40, weightHigh = 15 };

    public static RoguelikePlantTierWeights StsBoss() =>
        new RoguelikePlantTierWeights { weightLow = 20, weightMid = 35, weightHigh = 45 };
}

[Serializable]
public class RoguelikePlantPoolFilter
{
    [LabelText("排除本 Run 已拥有")]
    public bool excludeOwnedInRun = true;

    [LabelText("同屏候选不重复")]
    public bool excludeDuplicatesInOffer = true;

    [LabelText("排除升级前置卡（LevelUp_Limit）")]
    public bool excludeLevelUpPlants = true;

    [LabelText("黑名单 chessName")]
    public List<string> blacklistCreatorIds = new List<string>();

    [LabelText("白名单池（非空时仅用此列表）")]
    public List<PropertyCreator> whitelistPool = new List<PropertyCreator>();
}

[Serializable]
public class RoguelikePlantPickRule
{
    [LabelText("关卡类型")]
    public RoguelikeLevelKind kind = RoguelikeLevelKind.Elite;

    [LabelText("启用")]
    public bool enabled = true;

    [LabelText("给 PlantPick 条目")]
    public bool grantPlantPick = true;

    [LabelText("展示数量")]
    [MinValue(1)]
    public int optionCount = 3;

    [LabelText("档位权重")]
    public RoguelikePlantTierWeights tierWeights = new RoguelikePlantTierWeights();
}
