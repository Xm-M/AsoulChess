using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 肉鸽剧情事件关结算：发放道具奖励并回到地图。
/// </summary>
[System.Serializable]
public class LevelOutCome_RoguelikeEventStory : ILevelOutcome
{
    [Tooltip("为空时从当前 RoguelikeEventDefinition.propRewardIds 读取")]
    public List<string> propRewardIds = new List<string>();

    public void HandleOutcome(bool win, Vector3 lastZombiePos)
    {
        if (!win)
            return;

        ApplyPropRewards();
        RoguelikeRunService.LeaveStoryEventNode();
    }

    void ApplyPropRewards()
    {
        var ids = propRewardIds;
        if (ids == null || ids.Count == 0)
        {
            var pending = RoguelikeRunService.ActiveStoryEvent;
            if (pending?.propRewardIds != null && pending.propRewardIds.Count > 0)
                ids = pending.propRewardIds;
        }

        if (ids == null)
            return;

        for (int i = 0; i < ids.Count; i++)
        {
            if (!string.IsNullOrEmpty(ids[i]))
                RoguelikeRunPropPool.AddProp(ids[i]);
        }
    }
}
