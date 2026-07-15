using System;
using UnityEngine;

/// <summary>将 <see cref="RoguelikeRestOptionDefinition"/> 转为运行时 <see cref="RoguelikeRestOption"/>。</summary>
public static class RoguelikeRestOptionFactory
{
    public static RoguelikeRestOption FromDefinition(
        RoguelikeRestOptionDefinition def,
        RoguelikeRunState state,
        RoguelikeEconomyConfig economy)
    {
        if (def == null || state == null)
            return null;

        string id = string.IsNullOrEmpty(def.optionId) ? def.name : def.optionId;
        economy ??= RoguelikeRunService.ResolveEconomyConfig();

        switch (def.effectKind)
        {
            case RoguelikeRestEffectKind.LawnMowerBonus:
                return BuildLawnMowerOption(def, id, state, economy);
            case RoguelikeRestEffectKind.LoadoutSlotBonus:
                return BuildLoadoutOption(def, id, state, economy);
            default:
                Debug.LogWarning($"[RoguelikeRestOptionFactory] 未支持的效果: {def.effectKind} ({def.name})");
                return null;
        }
    }

    static RoguelikeRestOption BuildLawnMowerOption(
        RoguelikeRestOptionDefinition def,
        string id,
        RoguelikeRunState state,
        RoguelikeEconomyConfig economy)
    {
        int bonus = ResolveBonus(def, economy?.GetRestLawnMowerBonus() ?? 2);
        int current = state.runLawnMowerCount > 0
            ? state.runLawnMowerCount
            : economy.GetInitialLawnMowerCount();

        return new RoguelikeRestOption
        {
            id = id,
            title = def.title,
            description = string.IsNullOrWhiteSpace(def.description)
                ? $"小推车 +{bonus}（当前 {current} → {current + bonus}）"
                : def.description,
            icon = def.icon,
            enabled = true,
            execute = (s, _) => ExecuteLawnMowerRest(s, economy, bonus),
        };
    }

    static RoguelikeRestOption BuildLoadoutOption(
        RoguelikeRestOptionDefinition def,
        string id,
        RoguelikeRunState state,
        RoguelikeEconomyConfig economy)
    {
        int bonus = ResolveBonus(def, economy?.GetRestLoadoutSlotBonus() ?? 1);
        int current = state.GetLoadoutSlotCount(economy);
        int max = economy.GetMaxLoadoutSlotCount();
        bool atCap = current >= max;

        return new RoguelikeRestOption
        {
            id = id,
            title = def.title,
            description = string.IsNullOrWhiteSpace(def.description)
                ? atCap
                    ? $"携带格已达上限（{max}）"
                    : $"携带格 +{bonus}（当前 {current} → {Mathf.Min(current + bonus, max)}）"
                : def.description,
            icon = def.icon,
            enabled = !atCap,
            execute = (s, _) => ExecuteExpandLoadout(s, economy, bonus),
        };
    }

    static int ResolveBonus(RoguelikeRestOptionDefinition def, int economyDefault) =>
        def.effectValue > 0 ? def.effectValue : economyDefault;

    static bool ExecuteLawnMowerRest(RoguelikeRunState state, RoguelikeEconomyConfig economy, int bonus)
    {
        if (state == null)
            return false;

        if (state.runLawnMowerCount <= 0)
            state.runLawnMowerCount = economy.GetInitialLawnMowerCount();

        state.runLawnMowerCount += bonus;
        return true;
    }

    static bool ExecuteExpandLoadout(RoguelikeRunState state, RoguelikeEconomyConfig economy, int bonus)
    {
        if (state == null)
            return false;

        int current = state.GetLoadoutSlotCount(economy);
        int max = economy.GetMaxLoadoutSlotCount();
        if (current >= max)
            return false;

        state.runLoadoutSlotCount = Mathf.Min(current + bonus, max);
        return true;
    }
}
