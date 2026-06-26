using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum RoguelikeNodeVisualState
{
    Locked,
    Selectable,
    Current,
    Cleared,
    Visited,
}

/// <summary>地图节点 UI；推荐做成预制体并赋给 <see cref="RoguelikeMapPanel.nodePrefab"/>。</summary>
public class RoguelikeMapNodeWidget : MonoBehaviour
{
    [Tooltip("节点底图 / 按钮 Target Graphic")]
    public Image background;

    [Tooltip("可选：单独图标层；未配置时可用 background 显示 iconSprite")]
    public Image iconImage;

    public Button button;
    public TMP_Text label;

    int _nodeId;
    Action<int> _onClick;

    public int BoundNodeId => _nodeId;

    public void Bind(
        int nodeId,
        MapRoomType roomType,
        RoguelikeNodeVisualState state,
        Action<int> onClick,
        RoguelikeMapNodeTypeStyle typeStyle,
        RoguelikeMapNodeStateStyle stateStyle,
        bool showLabelWhenIcon)
    {
        _nodeId = nodeId;
        _onClick = onClick;

        typeStyle ??= RoguelikeMapVisualSettingsAsset.CreateDefaultTypeStyle(roomType);
        stateStyle ??= new RoguelikeMapNodeStateStyle();

        bool hasIcon = typeStyle.iconSprite != null;
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(hasIcon);
            if (hasIcon)
            {
                iconImage.sprite = typeStyle.iconSprite;
                iconImage.color = ResolveIconColor(state, typeStyle.baseColor, stateStyle);
            }
        }

        if (background != null)
        {
            if (typeStyle.backgroundSprite != null)
                background.sprite = typeStyle.backgroundSprite;
            if (hasIcon && iconImage == null)
            {
                background.sprite = typeStyle.iconSprite;
                background.color = ResolveIconColor(state, typeStyle.baseColor, stateStyle);
            }
            else
                background.color = ResolveBackgroundColor(state, typeStyle.baseColor, stateStyle);
        }

        if (label != null)
        {
            bool showLabel = !hasIcon || showLabelWhenIcon;
            label.gameObject.SetActive(showLabel);
            if (showLabel)
            {
                label.text = string.IsNullOrEmpty(typeStyle.labelOverride)
                    ? DefaultLabel(roomType)
                    : typeStyle.labelOverride;
            }
        }

        if (button != null)
        {
            bool clickable = state == RoguelikeNodeVisualState.Selectable
                             || state == RoguelikeNodeVisualState.Current;
            button.interactable = clickable;
            button.onClick.RemoveAllListeners();
            if (clickable && state == RoguelikeNodeVisualState.Selectable)
                button.onClick.AddListener(OnClick);
        }
    }

    public void SetSize(Vector2 size)
    {
        var rt = transform as RectTransform;
        if (rt != null)
            rt.sizeDelta = size;
    }

    void OnClick() => _onClick?.Invoke(_nodeId);

    static string DefaultLabel(MapRoomType t)
    {
        return t switch
        {
            MapRoomType.Start => "起",
            MapRoomType.Normal => "战",
            MapRoomType.Elite => "精",
            MapRoomType.Boss => "Boss",
            MapRoomType.Rest => "休",
            MapRoomType.Shop => "店",
            MapRoomType.Event => "？",
            _ => "?",
        };
    }

    /// <summary>图标保持原色，便于看清远处节点类型（底图承担状态区分）。</summary>
    static Color ResolveIconColor(RoguelikeNodeVisualState state, Color baseColor, RoguelikeMapNodeStateStyle s)
    {
        if (state == RoguelikeNodeVisualState.Current)
            return Color.Lerp(baseColor, Color.white, s.currentHighlightLerp);
        return baseColor;
    }

    static Color ResolveBackgroundColor(RoguelikeNodeVisualState state, Color baseColor, RoguelikeMapNodeStateStyle s)
    {
        return state switch
        {
            RoguelikeNodeVisualState.Selectable => baseColor * s.selectableTint,
            RoguelikeNodeVisualState.Current => Color.Lerp(baseColor, Color.white, s.currentHighlightLerp),
            RoguelikeNodeVisualState.Cleared => baseColor * s.clearedTint,
            RoguelikeNodeVisualState.Visited => baseColor * s.visitedTint,
            _ => s.lockedColor,
        };
    }

    /// <summary>未配置 nodePrefab 时由面板代码生成简易节点。</summary>
    public static RoguelikeMapNodeWidget CreateRuntime(Transform parent, Vector2 size, float labelFontSize)
    {
        var root = new GameObject("MapNode", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        root.transform.SetParent(parent, false);
        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = size;

        var img = root.GetComponent<Image>();
        img.raycastTarget = true;

        var btn = root.GetComponent<Button>();
        btn.targetGraphic = img;

        var textGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(root.transform, false);
        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        var tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = labelFontSize > 0 ? labelFontSize : size.y * 0.32f;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        var w = root.AddComponent<RoguelikeMapNodeWidget>();
        w.background = img;
        w.button = btn;
        w.label = tmp;
        return w;
    }
}
