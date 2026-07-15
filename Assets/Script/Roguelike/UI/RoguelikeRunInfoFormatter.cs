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

    public static string FormatRunGold(RoguelikeRunState state) =>
        state == null ? "0" : state.runGold.ToString();

    public static string FormatProgress(RoguelikeRunState state)
    {
        if (state?.currentMap?.nodes == null)
            return "0 / 0";

        int total = state.currentMap.nodes.Count;
        int visited = state.visitedNodeIds?.Count ?? 0;
        int cleared = state.clearedNodeIds?.Count ?? 0;
        return $"抵达 {visited} · 通关 {cleared} · 地图 {total}";
    }

    public static string FormatLawnMowerCount(RoguelikeRunState state)
    {
        if (state == null)
            return "0";

        int count = state.runLawnMowerCount;
        if (count <= 0)
        {
            var economy = RoguelikeRunService.ResolveEconomyConfig();
            count = economy.GetInitialLawnMowerCount();
        }

        return count.ToString();
    }

    public static string FormatLoadoutSlotCount(RoguelikeRunState state)
    {
        if (state == null)
            return "0";

        var economy = RoguelikeRunService.ResolveEconomyConfig();
        int current = state.GetLoadoutSlotCount(economy);
        int max = economy.GetMaxLoadoutSlotCount();
        return $"{current}/{max}";
    }

    /// <summary>当前地图深度层（1-based），无节点时返回 0。</summary>
    public static int ResolveMapLayerNumber(RoguelikeRunState state, int preferredNodeId = -1)
    {
        if (state?.currentMap == null)
            return 0;

        int nodeId = preferredNodeId >= 0 ? preferredNodeId : state.currentNodeId;
        if (nodeId < 0)
            return 0;

        var node = state.currentMap.GetNode(nodeId);
        return node != null ? node.layer + 1 : 0;
    }

    public static string FormatMapLayer(RoguelikeRunState state)
    {
        int layer = ResolveMapLayerNumber(state);
        return layer > 0 ? $"L{layer}" : "—";
    }

    static string FormatActDisplayName(RoguelikeRunState state, RunMapConfig config)
    {
        if (state == null)
            return string.Empty;

        var act = config?.GetAct(state.currentActIndex);
        if (act != null && !string.IsNullOrEmpty(act.displayName))
            return act.displayName;

        return $"Act {state.currentActIndex + 1}";
    }

    /// <summary>肉鸽战斗 ProgressBar 标题：地图名 · L层 · 关卡名。</summary>
    public static string FormatProgressBarStageName(LevelData level)
    {
        if (level == null)
            return string.Empty;

        if (!RoguelikeRunService.HasActiveRun
            || level.roguelikeKind == RoguelikeLevelKind.None)
        {
            return level.levelName ?? string.Empty;
        }

        var state = RoguelikeRunService.State;
        if (state == null)
            return level.levelName ?? string.Empty;

        string actName = FormatActDisplayName(state, RoguelikeRunService.ActiveRunConfig);
        int nodeId = RoguelikeRunService.PendingNodeId >= 0
            ? RoguelikeRunService.PendingNodeId
            : state.currentNodeId;
        int layer = ResolveMapLayerNumber(state, nodeId);

        if (string.IsNullOrEmpty(actName))
            return layer > 0 ? $"L{layer} · {level.levelName}" : level.levelName;

        if (layer > 0)
            return $"{actName} · L{layer} · {level.levelName}";

        return $"{actName} · {level.levelName}";
    }

    public static string FormatHudStatTooltip(RoguelikeRunInfoHudStatKind kind, RoguelikeRunState state)
    {
        switch (kind)
        {
            case RoguelikeRunInfoHudStatKind.Gold:
                return "金币\n" +
                       $"当前：{FormatRunGold(state)}\n" +
                       "本 Run 内持有，用于地图商店购买植物。战斗搜刮与部分奖励会增加金币。";

            case RoguelikeRunInfoHudStatKind.Deck:
                int plantCount = state?.ownedPlantCreatorIds?.Count ?? 0;
                return "植物卡组\n" +
                       $"已拥有：{plantCount} 种\n" +
                       "点击查看本 Run 已获得的所有植物。进战斗前从中选择本关 loadout。";

            case RoguelikeRunInfoHudStatKind.LawnMower:
                return "小推车\n" +
                       $"拥有：{FormatLawnMowerCount(state)}\n" +
                       "每场战斗最多生成 Min(地图行数, 拥有数) 台。\n" +
                       "战斗中损毁的小推车在过关奖励出现时从 Run 池扣除；休息房可 +2。";

            case RoguelikeRunInfoHudStatKind.LoadoutSlot:
                return "携带格\n" +
                       $"当前：{FormatLoadoutSlotCount(state)}\n" +
                       "每场战斗选卡时最多能带上场的植物张数（与卡组仓库区分）。\n" +
                       "休息房「扩容」可 +1，上限由配置决定。";

            case RoguelikeRunInfoHudStatKind.MapLayer:
                return "地图层数\n" +
                       $"当前：{FormatMapLayer(state)}\n" +
                       "本 Act 地图从左到右推进；数字越大越接近 Boss。";

            default:
                return string.Empty;
        }
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
