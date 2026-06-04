using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>单道具配置（ScriptableObject）。</summary>
[CreateAssetMenu(fileName = "NewProp", menuName = "Prop/PropItemData")]
public class PropItemData : ScriptableObject
{
    [LabelText("道具ID")]
    [Tooltip("存档 ownedPropIds 与查表用；留空则用资源名")]
    public string propId;

    [LabelText("显示名称")]
    public string displayName;

    [LabelText("图标")]
    public Sprite icon;

    [LabelText("效果说明")]
    [Multiline]
    public string effectDescription;

    [LabelText("进局效果")]
    [SerializeReference]
    public PropEffect effect;

    public string GetPropId() => !string.IsNullOrEmpty(propId) ? propId : name;
}
