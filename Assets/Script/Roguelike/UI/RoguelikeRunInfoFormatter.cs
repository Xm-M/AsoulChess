using System.Text;

/// <summary>将 <see cref="RoguelikeRunState"/> 格式化为 UI 文案。</summary>
public static class RoguelikeRunInfoFormatter
{
    public static string FormatRoomType(MapRoomType type)
    {
        switch (type)
        {
            case MapRoomType.Start: return "起点";
            case MapRoomType.Normal: return "普通战斗";
            case MapRoomType.Elite: return "精英";
            case MapRoomType.Boss: return "Boss";
            case MapRoomType.Rest: return "休息";
            case MapRoomType.Shop: return "商店";
            case MapRoomType.Event: return "事件";
            default: return type.ToString();
        }
    }

    public static string FormatActTitle(RoguelikeRunState state, RunMapConfig config)
    {
        if (state == null)
            return string.Empty;

        var act = config?.GetAct(state.currentActIndex);
        string actName = act != null && !string.IsNullOrEmpty(act.displayName)
            ? act.displayName
            : $"Act {state.currentActIndex + 1}";
        int totalActs = config != null ? config.ActCount : 0;
        return totalActs > 0
            ? $"{actName} ({state.currentActIndex + 1}/{totalActs})"
            : actName;
    }

    public static string FormatCurrentNode(RoguelikeRunState state)
    {
        var node = state?.CurrentNode;
        if (node == null)
            return "—";

        return $"L{node.layer + 1} · {FormatRoomType(node.roomType)} · #{node.id}";
    }

    public static string FormatProgress(RoguelikeRunState state)
    {
        if (state?.currentMap?.nodes == null)
            return "0 / 0";

        int total = state.currentMap.nodes.Count;
        int visited = state.visitedNodeIds?.Count ?? 0;
        int cleared = state.clearedNodeIds?.Count ?? 0;
        return $"抵达 {visited} · 通关 {cleared} · 地图 {total}";
    }

    public static string FormatPendingNode(RoguelikeRunState state, int pendingNodeId)
    {
        if (pendingNodeId < 0 || state?.currentMap == null)
            return "无";

        var node = state.currentMap.GetNode(pendingNodeId);
        if (node == null)
            return "无";

        return $"L{node.layer + 1} · {FormatRoomType(node.roomType)} · #{node.id}";
    }

    public static string FormatSelectableNext(RoguelikeRunState state)
    {
        if (state == null)
            return "0";

        var next = state.GetSelectableNextNodes();
        if (next == null || next.Count == 0)
            return "0";

        var sb = new StringBuilder();
        sb.Append(next.Count).Append("：");
        for (int i = 0; i < next.Count; i++)
        {
            if (i > 0)
                sb.Append(" / ");
            sb.Append(FormatRoomType(next[i].roomType));
        }
        return sb.ToString();
    }
}
