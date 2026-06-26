using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 肉鸽战斗胜利奖励：生成条目列表，由 <see cref="RoguelikeRewardPanel"/> 逐条领取。
/// 休息/商店不进战斗，不经过本类。
/// </summary>
public static class RoguelikeRewardFlow
{
    public static List<RoguelikeRewardEntry> BuildCombatRewardEntries(LevelData level)
    {
        var list = new List<RoguelikeRewardEntry>();
        if (!RoguelikeRunService.HasActiveRun || level == null)
            return list;

        var kind = level.roguelikeKind;
        if (kind == RoguelikeLevelKind.None)
        {
            Debug.LogWarning(
                $"[RoguelikeRewardFlow] 关卡 {level.levelName} 未设置 roguelikeKind，无战斗奖励条目");
            return list;
        }

        var economy = RoguelikeRunService.ActiveRunConfig?.economyConfig
                      ?? RoguelikeEconomyConfig.CreateRuntimeDefaults();

        int gold = economy.RollGold(kind, RoguelikeRunService.State, level);
        if (gold > 0)
            list.Add(RoguelikeRewardEntry.CreateGold(gold));

        if (economy.ShouldGrantPlantPick(kind))
        {
            var plantOptions = economy.RollPlantPickOptions(kind, RoguelikeRunService.State, level);
            if (plantOptions != null && plantOptions.Count > 0)
                list.Add(RoguelikeRewardEntry.CreatePlantPick(plantOptions));
        }

        if (economy.ShouldGrantPropReward(kind))
        {
            var propId = economy.RollPropReward(kind, RoguelikeRunService.State, level);
            if (!string.IsNullOrEmpty(propId))
                list.Add(RoguelikeRewardEntry.CreateProp(propId));
        }

        return list;
    }

    public static bool TryClaimEntry(RoguelikeRewardEntry entry)
    {
        if (entry == null || entry.claimed || RoguelikeRunService.State == null)
            return false;

        switch (entry.kind)
        {
            case RoguelikeRewardEntryKind.Gold:
                if (entry.goldAmount <= 0)
                    return false;
                RoguelikeRunService.State.runGold += entry.goldAmount;
                entry.claimed = true;
                RoguelikeRunService.SaveRun();
                Debug.Log(
                    $"[RoguelikeRewardFlow] 领取 {entry.goldAmount} 金币，当前 {RoguelikeRunService.State.runGold}");
                return true;
            case RoguelikeRewardEntryKind.PlantPick:
                Debug.LogWarning($"[RoguelikeRewardFlow] 奖励类型 {entry.kind} 请走专用领取流程");
                return false;
            case RoguelikeRewardEntryKind.Item:
                return TryClaimProp(entry);
            default:
                return false;
        }
    }

    /// <summary>PlantPick 子面板选卡或跳过后领取。<paramref name="chosenChessName"/> 为空表示跳过。</summary>
    public static bool TryClaimPlantPick(RoguelikeRewardEntry entry, string chosenChessName)
    {
        if (entry == null || entry.claimed || RoguelikeRunService.State == null)
            return false;
        if (entry.kind != RoguelikeRewardEntryKind.PlantPick)
            return false;

        if (!string.IsNullOrEmpty(chosenChessName))
        {
            if (entry.plantPickOptions == null || !entry.plantPickOptions.Contains(chosenChessName))
                return false;

            if (!RoguelikeRunPlantPool.AddPlant(chosenChessName))
                Debug.LogWarning(
                    $"[RoguelikeRewardFlow] 植物 {chosenChessName} 加入牌组失败（可能已拥有）");
            else
                Debug.Log($"[RoguelikeRewardFlow] 选择植物 {chosenChessName}");
        }
        else
            Debug.Log("[RoguelikeRewardFlow] 跳过 PlantPick");

        entry.claimed = true;
        RoguelikeRunService.SaveRun();
        return true;
    }

    public static bool TryClaimProp(RoguelikeRewardEntry entry)
    {
        if (entry == null || entry.claimed || RoguelikeRunService.State == null)
            return false;
        if (entry.kind != RoguelikeRewardEntryKind.Item)
            return false;
        if (string.IsNullOrEmpty(entry.propId))
            return false;

        if (!RoguelikeRunPropPool.AddProp(entry.propId))
        {
            Debug.LogWarning($"[RoguelikeRewardFlow] 道具 {entry.propId} 加入 Run 失败（可能已拥有）");
            return false;
        }

        Debug.Log($"[RoguelikeRewardFlow] 获得道具 {entry.propId}");
        entry.claimed = true;
        RoguelikeRunService.SaveRun();
        return true;
    }
}
