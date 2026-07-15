using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 休息房单张选项卡配置（策划在 Project 中创建，可配图）。
/// 由 <see cref="RoguelikeRestOptionCatalog"/> 引用，经 <see cref="RoguelikeRestFlow"/> 展示。
/// </summary>
[CreateAssetMenu(fileName = "RestOption", menuName = "Roguelike/Rest Option")]
public class RoguelikeRestOptionDefinition : ScriptableObject
{
    [LabelText("选项 Id")]
    [Tooltip("全 Run 唯一，如 builtin.rest")]
    public string optionId = "rest.option";

    [LabelText("标题")]
    public string title = "选项";

    [LabelText("描述")]
    [Tooltip("留空则按效果自动生成（含当前→变化后）")]
    [TextArea(2, 4)]
    public string description;

    [LabelText("配图")]
    public Sprite icon;

    [LabelText("效果类型")]
    public RoguelikeRestEffectKind effectKind = RoguelikeRestEffectKind.LawnMowerBonus;

    [LabelText("效果数值")]
    [Tooltip("≤0 时使用 RoguelikeEconomyConfig 对应休息加成")]
    [MinValue(0)]
    public int effectValue;
}
