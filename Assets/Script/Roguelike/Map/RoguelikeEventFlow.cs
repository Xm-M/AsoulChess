using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 从 <see cref="ActMapConfig.eventStoryPool"/> 抽取剧情事件并解析 StoryMode 关卡。
/// </summary>
public static class RoguelikeEventFlow
{
    public static RoguelikeEventDefinition PickStoryEvent(
        List<RoguelikeEventDefinition> pool,
        RoguelikeMapNode node,
        int runSeed)
    {
        if (pool == null || pool.Count == 0 || node == null)
            return null;

        var valid = new List<RoguelikeEventDefinition>();
        for (int i = 0; i < pool.Count; i++)
        {
            var e = pool[i];
            if (e != null && e.storyLevel != null)
                valid.Add(e);
        }

        if (valid.Count == 0)
            return null;

        int idx = Mathf.Abs(runSeed + node.id * 47) % valid.Count;
        return valid[idx];
    }

    public static RoguelikeEventDefinition ResolveStoryEventForPendingNode()
    {
        if (!RoguelikeRunService.HasActiveRun || RoguelikeRunService.PendingNodeId < 0)
            return null;

        var state = RoguelikeRunService.State;
        var node = state?.currentMap?.GetNode(RoguelikeRunService.PendingNodeId);
        var act = RoguelikeRunService.ActiveRunConfig?.GetAct(state.currentActIndex);
        if (act?.eventStoryPool == null || act.eventStoryPool.Count == 0)
        {
            Debug.LogWarning("[Roguelike] eventStoryPool 为空，无法进入剧情事件");
            return null;
        }

        return PickStoryEvent(act.eventStoryPool, node, state.runSeed);
    }
}
