using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 肉鸽地图休息房：构建选项、执行选择。不进战斗，不经过 <see cref="RoguelikeRewardFlow"/>。
/// </summary>
public static class RoguelikeRestFlow
{
    public static List<RoguelikeRestOption> BuildOptions(int nodeId)
    {
        var list = new List<RoguelikeRestOption>();
        var state = RoguelikeRunService.State;
        if (state == null || nodeId < 0)
            return list;

        if (!state.IsRestChoiceUsed(nodeId))
        {
            list.Add(CreateLawnMowerRestOption(state));
            list.Add(CreateExpandLoadoutOption(state));
        }

        RoguelikeRestOptionRegistry.AppendAll(nodeId, state, list);
        return list;
    }

    public static bool TryChooseOption(int nodeId, string optionId)
    {
        var state = RoguelikeRunService.State;
        if (state == null || nodeId < 0 || string.IsNullOrEmpty(optionId))
            return false;
        if (state.IsRestChoiceUsed(nodeId))
            return false;

        var options = BuildOptions(nodeId);
        RoguelikeRestOption target = null;
        for (int i = 0; i < options.Count; i++)
        {
            var o = options[i];
            if (o != null && o.id == optionId)
            {
                target = o;
                break;
            }
        }

        if (target == null || !target.enabled || target.execute == null)
            return false;

        if (!target.execute(state, nodeId))
            return false;

        state.MarkRestChoiceUsed(nodeId);
        RoguelikeRunService.SaveRun();
        Debug.Log($"[RoguelikeRestFlow] 休息选项 {optionId}（节点 {nodeId}）");
        return true;
    }

    static RoguelikeRestOption CreateLawnMowerRestOption(RoguelikeRunState state)
    {
        var economy = RoguelikeRunService.ResolveEconomyConfig();
        int bonus = economy.GetRestLawnMowerBonus();
        int current = state.runLawnMowerCount > 0
            ? state.runLawnMowerCount
            : economy.GetInitialLawnMowerCount();

        return new RoguelikeRestOption
        {
            id = RoguelikeRestOption.BuiltinRestId,
            title = "休息",
            description = $"小推车 +{bonus}（当前 {current} → {current + bonus}）",
            enabled = true,
            execute = ExecuteLawnMowerRest,
        };
    }

    static bool ExecuteLawnMowerRest(RoguelikeRunState state, int nodeId)
    {
        if (state == null)
            return false;

        var economy = RoguelikeRunService.ResolveEconomyConfig();
        if (state.runLawnMowerCount <= 0)
            state.runLawnMowerCount = economy.GetInitialLawnMowerCount();

        state.runLawnMowerCount += economy.GetRestLawnMowerBonus();
        return true;
    }

    static RoguelikeRestOption CreateExpandLoadoutOption(RoguelikeRunState state)
    {
        var economy = RoguelikeRunService.ResolveEconomyConfig();
        int current = state.GetLoadoutSlotCount(economy);
        int max = economy.GetMaxLoadoutSlotCount();
        int bonus = economy.GetRestLoadoutSlotBonus();
        bool atCap = current >= max;

        return new RoguelikeRestOption
        {
            id = RoguelikeRestOption.BuiltinExpandId,
            title = "扩容",
            description = atCap
                ? $"携带格已达上限（{max}）"
                : $"携带格 +{bonus}（当前 {current} → {Mathf.Min(current + bonus, max)}）",
            enabled = !atCap,
            execute = ExecuteExpandLoadout,
        };
    }

    static bool ExecuteExpandLoadout(RoguelikeRunState state, int nodeId)
    {
        if (state == null)
            return false;

        var economy = RoguelikeRunService.ResolveEconomyConfig();
        int current = state.GetLoadoutSlotCount(economy);
        int max = economy.GetMaxLoadoutSlotCount();
        if (current >= max)
            return false;

        state.runLoadoutSlotCount = Mathf.Min(current + economy.GetRestLoadoutSlotBonus(), max);
        return true;
    }
}
