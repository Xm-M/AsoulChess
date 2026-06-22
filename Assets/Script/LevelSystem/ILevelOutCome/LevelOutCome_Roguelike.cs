using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 肉鸽战斗关胜利/失败流程。
/// 胜利：僵尸全灭 → 当场弹出奖励面板 → 领完/跳过 → 离开关卡 → 回地图选路。
/// </summary>
[System.Serializable]
public class LevelOutCome_Roguelike : ILevelOutcome
{
    public void HandleOutcome(bool win, Vector3 lastZombiePos)
    {
        if (!win)
        {
            RoguelikeRunService.OnCombatFinished(false);
            RoguelikeRunService.ReturnToMapUI();
            return;
        }

        var entries = RoguelikeRewardFlow.BuildCombatRewardEntries(LevelManage.instance?.currentLevel);
        ShowRewardsThenLeaveLevel(entries);
    }

    static void ShowRewardsThenLeaveLevel(List<RoguelikeRewardEntry> entries)
    {
        if (entries == null || entries.Count == 0)
        {
            LeaveLevelAndOpenMap();
            return;
        }

        RoguelikeRewardPanel.ShowRewards(entries, LeaveLevelAndOpenMap);
    }

    static void LeaveLevelAndOpenMap()
    {
        RoguelikeRunService.OnCombatFinished(true);
        RoguelikeRunService.ReturnToMapUI();
    }
}
