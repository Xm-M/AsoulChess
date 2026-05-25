using UnityEngine;

/// <summary>
/// 肉鸽战斗关胜利/失败后回到地图流程。挂在肉鸽专用 <see cref="LevelData.outcome"/> 上。
/// 胜利调用 <see cref="RoguelikeRunService.OnCombatFinished"/>；失败结束整局（可按策划改为扣命继续）。
/// </summary>
[System.Serializable]
public class LevelOutCome_Roguelike : ILevelOutcome
{
    public void HandleOutcome(bool win, Vector3 lastZombiePos)
    {
        RoguelikeRunService.OnCombatFinished(win);
        RoguelikeRunService.ReturnToMapUI();
    }
}
