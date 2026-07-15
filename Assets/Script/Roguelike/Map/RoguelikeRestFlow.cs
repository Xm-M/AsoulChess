using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 肉鸽地图休息房：构建选项、执行选择。不进战斗，不经过 <see cref="RoguelikeRewardFlow"/>。
/// 选项内容来自 <see cref="RoguelikeEconomyConfig.restOptionCatalog"/>（策划配置 + 配图）。
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
            AppendCatalogOptions(state, list);

        RoguelikeRestOptionRegistry.AppendAll(nodeId, state, list);
        return list;
    }

    static void AppendCatalogOptions(RoguelikeRunState state, List<RoguelikeRestOption> list)
    {
        var economy = RoguelikeRunService.ResolveEconomyConfig();
        var catalog = economy?.restOptionCatalog;
        if (catalog != null)
        {
            catalog.AppendRuntimeOptions(state, economy, list);
            return;
        }

        Debug.LogWarning(
            "[RoguelikeRestFlow] RoguelikeEconomyConfig.restOptionCatalog 未配置，使用代码内置休息/扩容（无配图）。");
        AppendLegacyBuiltinOptions(state, economy, list);
    }

    static void AppendLegacyBuiltinOptions(
        RoguelikeRunState state,
        RoguelikeEconomyConfig economy,
        List<RoguelikeRestOption> list)
    {
        var restDef = ScriptableObject.CreateInstance<RoguelikeRestOptionDefinition>();
        restDef.optionId = RoguelikeRestOption.BuiltinRestId;
        restDef.title = "休息";
        restDef.effectKind = RoguelikeRestEffectKind.LawnMowerBonus;
        restDef.effectValue = 0;

        var expandDef = ScriptableObject.CreateInstance<RoguelikeRestOptionDefinition>();
        expandDef.optionId = RoguelikeRestOption.BuiltinExpandId;
        expandDef.title = "扩容";
        expandDef.effectKind = RoguelikeRestEffectKind.LoadoutSlotBonus;
        expandDef.effectValue = 0;

        var rest = RoguelikeRestOptionFactory.FromDefinition(restDef, state, economy);
        var expand = RoguelikeRestOptionFactory.FromDefinition(expandDef, state, economy);
        if (rest != null) list.Add(rest);
        if (expand != null) list.Add(expand);
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
}
