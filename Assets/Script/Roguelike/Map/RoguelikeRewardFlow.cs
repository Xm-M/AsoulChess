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
            case RoguelikeRewardEntryKind.Item:
                Debug.LogWarning($"[RoguelikeRewardFlow] 奖励类型 {entry.kind} 尚未实现");
                return false;
            default:
                return false;
        }
    }
}
