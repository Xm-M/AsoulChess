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
    [Tooltip("节点底图")]
    public Image background;

    [Tooltip("房间类型图标")]
    public Image iconImage;

    [Tooltip("状态遮罩（Locked / Visited / Cleared）")]
    public Image stateOverlay;

    [Tooltip("已通关 ✔")]
    public Image clearedCheckmark;

    public Button button;
    public TMP_Text label;

    [SerializeField] UIHoverScale hoverScale;

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

        Color typeColor = ResolveTypeColor(state, typeStyle.baseColor, stateStyle);
        bool hasIcon = typeStyle.iconSprite != null;

        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(hasIcon);
            if (hasIcon)
            {
                iconImage.sprite = typeStyle.iconSprite;
                iconImage.color = typeColor;
            }
        }

        if (background != null)
        {
            if (typeStyle.backgroundSprite != null)
                background.sprite = typeStyle.backgroundSprite;

            if (hasIcon && iconImage == null)
            {
                background.sprite = typeStyle.iconSprite;
                background.color = typeColor;
            }
            else
            {
                background.color = stateOverlay != null
                    ? typeColor
                    : ResolveLegacyBackgroundColor(state, typeColor, stateStyle);
            }
        }

        ApplyStateOverlay(state, stateStyle);
        ApplyClearedCheckmark(state, stateStyle);

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

        bool clickable = state == RoguelikeNodeVisualState.Selectable
                         || state == RoguelikeNodeVisualState.Current;
        if (button != null)
        {
            button.interactable = clickable;
            button.onClick.RemoveAllListeners();
            if (clickable && state == RoguelikeNodeVisualState.Selectable)
                button.onClick.AddListener(OnClick);
        }

        if (hoverScale == null)
            hoverScale = GetComponent<UIHoverScale>();
        hoverScale?.SetEnabled(state == RoguelikeNodeVisualState.Selectable
                              || state == RoguelikeNodeVisualState.Current);
    }

    public void SetSize(Vector2 size)
    {
        var rt = transform as RectTransform;
        if (rt != null)
            rt.sizeDelta = size;
    }

    void OnClick() => _onClick?.Invoke(_nodeId);

    void ApplyStateOverlay(RoguelikeNodeVisualState state, RoguelikeMapNodeStateStyle style)
    {
        if (stateOverlay == null)
            return;

        if (TryGetOverlayColor(state, style, out Color overlayColor))
        {
            stateOverlay.gameObject.SetActive(true);
            stateOverlay.color = overlayColor;
        }
        else
        {
            stateOverlay.gameObject.SetActive(false);
        }
    }

    void ApplyClearedCheckmark(RoguelikeNodeVisualState state, RoguelikeMapNodeStateStyle style)
    {
        if (clearedCheckmark == null)
            return;

        bool show = state == RoguelikeNodeVisualState.Cleared && style.clearedCheckmarkSprite != null;
        clearedCheckmark.gameObject.SetActive(show);
        if (!show)
            return;

        clearedCheckmark.sprite = style.clearedCheckmarkSprite;
        clearedCheckmark.color = Color.white;
    }

    static bool TryGetOverlayColor(
        RoguelikeNodeVisualState state,
        RoguelikeMapNodeStateStyle style,
        out Color color)
    {
        switch (state)
        {
            case RoguelikeNodeVisualState.Locked:
                color = style.lockedOverlayColor;
                return true;
            case RoguelikeNodeVisualState.Visited:
                color = style.visitedOverlayColor;
                return true;
            case RoguelikeNodeVisualState.Cleared:
                color = style.clearedOverlayColor;
                return true;
            default:
                color = default;
                return false;
        }
    }

    static Color ResolveTypeColor(
        RoguelikeNodeVisualState state,
        Color baseColor,
        RoguelikeMapNodeStateStyle style)
    {
        if (state == RoguelikeNodeVisualState.Current)
            return Color.Lerp(baseColor, Color.white, style.currentHighlightLerp);
        if (state == RoguelikeNodeVisualState.Selectable)
            return baseColor * style.selectableTint;
        return baseColor;
    }

    static Color ResolveLegacyBackgroundColor(
        RoguelikeNodeVisualState state,
        Color baseColor,
        RoguelikeMapNodeStateStyle style)
    {
        return state switch
        {
            RoguelikeNodeVisualState.Selectable => baseColor * style.selectableTint,
            RoguelikeNodeVisualState.Current => Color.Lerp(baseColor, Color.white, style.currentHighlightLerp),
            RoguelikeNodeVisualState.Cleared => baseColor * style.clearedTint,
            RoguelikeNodeVisualState.Visited => baseColor * style.visitedTint,
            _ => style.lockedColor,
        };
    }

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

    /// <summary>未配置 nodePrefab 时由面板代码生成简易节点。</summary>
    public static RoguelikeMapNodeWidget CreateRuntime(Transform parent, Vector2 size, float labelFontSize)
    {
        var root = new GameObject("MapNode", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(UIHoverScale));
        root.transform.SetParent(parent, false);
        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.pivot = new Vector2(0.5f, 0.5f);

        var img = root.GetComponent<Image>();
        img.raycastTarget = true;
        img.color = new Color(1f, 1f, 1f, 0.01f);

        var btn = root.GetComponent<Button>();
        btn.targetGraphic = img;

        var overlayGo = CreateStretchImageChild(root.transform, "StateOverlay", new Color(0f, 0f, 0f, 0.5f));
        overlayGo.SetActive(false);
        var overlayImg = overlayGo.GetComponent<Image>();

        var checkGo = CreateAnchoredImageChild(
            root.transform,
            "ClearedCheck",
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 0.5f),
            new Vector2(-8f, -8f),
            new Vector2(28f, 28f),
            Color.white);
        checkGo.SetActive(false);
        var checkImg = checkGo.GetComponent<Image>();

        var iconGo = CreateAnchoredImageChild(
            root.transform,
            "Icon",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            size * 0.72f,
            Color.white);
        iconGo.SetActive(false);
        var iconImg = iconGo.GetComponent<Image>();

        var bgGo = CreateStretchImageChild(root.transform, "Background", Color.white);
        bgGo.transform.SetAsFirstSibling();
        var bgImg = bgGo.GetComponent<Image>();

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
        w.background = bgImg;
        w.iconImage = iconImg;
        w.stateOverlay = overlayImg;
        w.clearedCheckmark = checkImg;
        w.button = btn;
        w.label = tmp;
        w.hoverScale = root.GetComponent<UIHoverScale>();
        return w;
    }

    static GameObject CreateStretchImageChild(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var childRt = go.GetComponent<RectTransform>();
        childRt.anchorMin = Vector2.zero;
        childRt.anchorMax = Vector2.one;
        childRt.offsetMin = Vector2.zero;
        childRt.offsetMax = Vector2.zero;
        var childImg = go.GetComponent<Image>();
        childImg.color = color;
        childImg.raycastTarget = false;
        return go;
    }

    static GameObject CreateAnchoredImageChild(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var childRt = go.GetComponent<RectTransform>();
        childRt.anchorMin = anchorMin;
        childRt.anchorMax = anchorMax;
        childRt.pivot = pivot;
        childRt.anchoredPosition = anchoredPosition;
        childRt.sizeDelta = sizeDelta;
        var childImg = go.GetComponent<Image>();
        childImg.color = color;
        childImg.raycastTarget = false;
        childImg.preserveAspect = true;
        return go;
    }
}
