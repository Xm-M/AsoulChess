using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 肉鸽战斗关胜利/失败流程。
/// 胜利：僵尸全灭 → 当场弹出奖励面板 → 领完/跳过 → 离开关卡 → 回地图或全通关结算。
/// 失败：僵尸进家 → TextPanel 失败 UI → 玩家点「结算」→ 奖励面板结算视图 → 返回主菜单。
/// </summary>
[System.Serializable]
public class LevelOutCome_Roguelike : ILevelOutcome
{
    public void HandleOutcome(bool win, Vector3 lastZombiePos)
    {
        if (!win)
            return;

        // 奖励出现前结算（仅过关）；小推车实体尚未被 OverPlugin 销毁
        RoguelikeRunService.SettleCombatLawnMowerLosses();

        var entries = RoguelikeRewardFlow.BuildCombatRewardEntries(LevelManage.instance?.currentLevel);
        ShowRewardsThenLeaveLevel(entries);
    }

    static void ShowRewardsThenLeaveLevel(List<RoguelikeRewardEntry> entries)
    {
        if (entries == null || entries.Count == 0)
        {
            LeaveLevelAfterCombatVictory();
            return;
        }

        RoguelikeRewardPanel.ShowRewards(entries, LeaveLevelAfterCombatVictory);
    }

    static void LeaveLevelAfterCombatVictory()
    {
        if (RoguelikeRunService.FinalizeCombatVictory())
        {
            RoguelikeRewardPanel.ShowRunEnd(
                RoguelikeRunService.LastRunEndSummary,
                RoguelikeRunService.ReturnToStartAfterRunEnded);
            return;
        }

        RoguelikeRunService.ReturnToMapUI();
    }
}
