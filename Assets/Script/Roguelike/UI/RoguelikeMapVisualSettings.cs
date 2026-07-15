using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>连线样式（可在 <see cref="RoguelikeMapPanel"/> 或 <see cref="RoguelikeMapVisualSettingsAsset"/> 中配置）。</summary>
[Serializable]
public class RoguelikeMapLineStyle
{
    [Tooltip("线段粗细（像素）")]
    public float width = 4f;

    public Color color = new Color(0.75f, 0.78f, 0.85f, 0.9f);

    [Tooltip("可选；贴到 LineRenderer 材质 mainTexture（Tile 模式）")]
    public Sprite sprite;

    [Tooltip("为 true 且配置了 linePrefab 时，仅用 prefab 上 LineRenderer 的材质/宽度，忽略 width/color/sprite")]
    public bool usePrefabAppearanceOnly;
}

/// <summary>某一 <see cref="MapRoomType"/> 的节点默认外观。</summary>
[Serializable]
public class RoguelikeMapNodeTypeStyle
{
    public MapRoomType roomType = MapRoomType.Normal;

    [Tooltip("节点底图；为空则用纯色")]
    public Sprite backgroundSprite;

    [Tooltip("节点图标（可选）；配置后默认显示图标并隐藏文字")]
    public Sprite iconSprite;

    public Color baseColor = new Color(0.35f, 0.55f, 0.75f, 1f);

    [Tooltip("无图标时显示的文字；为空则用内置简写")]
    public string labelOverride;
}

/// <summary>节点状态（可选/当前/已通关等）对颜色的修饰。</summary>
[Serializable]
public class RoguelikeMapNodeStateStyle
{
    [Tooltip("不可达节点半透明遮罩（图标保持亮色，由 overlay 变暗）")]
    public Color lockedOverlayColor = new Color(0f, 0f, 0f, 0.55f);

    [Tooltip("已走过未通关")]
    public Color visitedOverlayColor = new Color(0f, 0f, 0f, 0.22f);

    [Tooltip("已通关")]
    public Color clearedOverlayColor = new Color(0f, 0f, 0f, 0.18f);

    [Tooltip("已通关 ✔ 图标；未配置则不显示")]
    public Sprite clearedCheckmarkSprite;

    [Tooltip("Legacy：CreateRuntime 无 overlay 时的 Locked 底色")]
    public Color lockedColor = new Color(0.25f, 0.25f, 0.28f, 0.85f);

    [Tooltip("与 baseColor 相乘")]
    public Color selectableTint = Color.white;

    [Tooltip("Current 状态图标/底图向白色插值比例")]
    [Range(0f, 1f)]
    public float currentHighlightLerp = 0.35f;

    [Tooltip("Legacy multiply tint；有 overlay 层时不再用于图标")]
    public Color clearedTint = new Color(0.55f, 0.55f, 0.55f, 1f);
    public Color visitedTint = new Color(0.7f, 0.7f, 0.7f, 0.9f);
}

/// <summary>可选：将一套视觉配置做成资产，多个面板或主题复用。</summary>
[CreateAssetMenu(fileName = "RoguelikeMapVisualSettings", menuName = "Roguelike/Map Visual Settings")]
public class RoguelikeMapVisualSettingsAsset : ScriptableObject
{
    public RoguelikeMapLineStyle lineStyle = new RoguelikeMapLineStyle();
    public RoguelikeMapNodeStateStyle nodeStateStyle = new RoguelikeMapNodeStateStyle();
    public List<RoguelikeMapNodeTypeStyle> nodeTypeStyles = new List<RoguelikeMapNodeTypeStyle>();

    public RoguelikeMapNodeTypeStyle GetTypeStyle(MapRoomType type)
    {
        if (nodeTypeStyles != null)
        {
            for (int i = 0; i < nodeTypeStyles.Count; i++)
            {
                if (nodeTypeStyles[i].roomType == type)
                    return nodeTypeStyles[i];
            }
        }
        return RoguelikeMapVisualSettingsAsset.CreateDefaultTypeStyle(type);
    }

    public static RoguelikeMapNodeTypeStyle CreateDefaultTypeStyle(MapRoomType type)
    {
        var s = new RoguelikeMapNodeTypeStyle { roomType = type };
        s.baseColor = type switch
        {
            MapRoomType.Elite => new Color(0.85f, 0.45f, 0.15f),
            MapRoomType.Boss => new Color(0.75f, 0.15f, 0.15f),
            MapRoomType.Rest => new Color(0.2f, 0.65f, 0.35f),
            MapRoomType.Shop => new Color(0.85f, 0.75f, 0.2f),
            MapRoomType.Event => new Color(0.55f, 0.35f, 0.85f),
            MapRoomType.Start => new Color(0.35f, 0.55f, 0.9f),
            _ => new Color(0.35f, 0.55f, 0.75f),
        };
        return s;
    }

    void Reset()
    {
        EnsureDefaultNodeTypes();
    }

    void OnValidate()
    {
        EnsureDefaultNodeTypes();
    }

    public void EnsureDefaultNodeTypes()
    {
        nodeTypeStyles ??= new List<RoguelikeMapNodeTypeStyle>();
        foreach (MapRoomType t in Enum.GetValues(typeof(MapRoomType)))
        {
            bool found = false;
            for (int i = 0; i < nodeTypeStyles.Count; i++)
            {
                if (nodeTypeStyles[i].roomType == t)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                nodeTypeStyles.Add(CreateDefaultTypeStyle(t));
        }
    }
}
