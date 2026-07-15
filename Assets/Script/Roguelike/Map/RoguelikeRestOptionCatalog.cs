using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 休息房选项列表。挂在 <see cref="RoguelikeEconomyConfig.restOptionCatalog"/>。
/// </summary>
[CreateAssetMenu(fileName = "RoguelikeRestOptionCatalog", menuName = "Roguelike/Rest Option Catalog")]
public class RoguelikeRestOptionCatalog : ScriptableObject
{
    [LabelText("休息房选项（顺序即展示顺序）")]
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<RoguelikeRestOptionDefinition> options = new List<RoguelikeRestOptionDefinition>();

    public void AppendRuntimeOptions(
        RoguelikeRunState state,
        RoguelikeEconomyConfig economy,
        List<RoguelikeRestOption> target)
    {
        if (target == null || state == null || options == null)
            return;

        for (int i = 0; i < options.Count; i++)
        {
            var def = options[i];
            if (def == null)
                continue;

            var option = RoguelikeRestOptionFactory.FromDefinition(def, state, economy);
            if (option != null)
                target.Add(option);
        }
    }
}
