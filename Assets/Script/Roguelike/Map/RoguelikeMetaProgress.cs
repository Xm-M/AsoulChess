/// <summary>
/// 将肉鸽 Run 进度写入 <see cref="PlayerSaveData"/>（Meta：最高层/Act、通关次数等）。
/// </summary>
public static class RoguelikeMetaProgress
{
    public static void ApplyFromRun(RoguelikeRunState state, bool runCompleted, bool actClearedThisSession)
    {
        if (state == null)
            return;

        var player = PlayerSaveContext.CurrentData ?? PlayerSaveContext.LoadCurrent();
        if (player == null)
            return;

        int maxLayer = ComputeMaxLayerReached(state);
        if (maxLayer > player.roguelikeBestLayerReached)
            player.roguelikeBestLayerReached = maxLayer;

        int actHigh = state.currentActIndex;
        if (actClearedThisSession)
            actHigh = state.currentActIndex + 1;
        if (actHigh > player.roguelikeBestActReached)
            player.roguelikeBestActReached = actHigh;

        if (runCompleted)
            player.roguelikeRunsCompleted++;

        if (actClearedThisSession)
            player.roguelikeActsCleared++;

        PlayerSaveContext.CurrentData = player;
        PlayerSaveContext.SaveCurrent();
    }

    static int ComputeMaxLayerReached(RoguelikeRunState state)
    {
        int max = 0;
        if (state.currentMap?.nodes == null)
            return max;

        for (int i = 0; i < state.clearedNodeIds.Count; i++)
        {
            var n = state.currentMap.GetNode(state.clearedNodeIds[i]);
            if (n != null && n.layer > max)
                max = n.layer;
        }

        for (int i = 0; i < state.visitedNodeIds.Count; i++)
        {
            var n = state.currentMap.GetNode(state.visitedNodeIds[i]);
            if (n != null && n.layer > max)
                max = n.layer;
        }

        var cur = state.CurrentNode;
        if (cur != null && cur.layer > max)
            max = cur.layer;

        return max;
    }
}
