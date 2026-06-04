using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>本局生效道具栏（对标 <see cref="FetterPanel"/>）。</summary>
public class PropPanel : View
{
    public GameObject propIconPrefab;
    public RectTransform propIconParent;

    [Header("道具说明（悬停/长按）")]
    [SerializeField] RectTransform tooltipRoot;
    [SerializeField] TMP_Text tooltipDescriptionText;
    [SerializeField] Vector2 tooltipScreenOffset = new Vector2(0f, 48f);

    readonly List<GameObject> icons = new List<GameObject>();
    RectTransform _tooltipFollowTarget;

    void Awake()
    {
        if (tooltipRoot != null)
            tooltipRoot.gameObject.SetActive(false);
    }

    public override void Init()
    {
        icons.Clear();
    }

    public void Refresh(IReadOnlyList<PropItemData> props)
    {
        ClearIcons();
        if (props == null || propIconPrefab == null || propIconParent == null) return;
        for (int i = 0; i < props.Count; i++)
        {
            if (props[i] == null) continue;
            var go = Instantiate(propIconPrefab, propIconParent);
            var icon = go.GetComponent<PropIcon>();
            icon?.ShowPropIcon(props[i]);
            icons.Add(go);
        }
    }

    public void ClearIcons()
    {
        HidePropTooltip();
        for (int i = icons.Count - 1; i >= 0; i--)
        {
            if (icons[i] != null)
                Destroy(icons[i]);
        }
        icons.Clear();
    }

    public override void Hide()
    {
        HidePropTooltip();
        ClearIcons();
        base.Hide();
    }

    public void ShowPropTooltip(RectTransform iconRect, string text)
    {
        if (tooltipRoot == null || tooltipDescriptionText == null || iconRect == null) return;
        _tooltipFollowTarget = iconRect;
        tooltipDescriptionText.text = text ?? "";
        tooltipRoot.gameObject.SetActive(true);
        SyncTooltipPosition();
    }

    public void HidePropTooltip()
    {
        _tooltipFollowTarget = null;
        if (tooltipRoot != null)
            tooltipRoot.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (tooltipRoot == null || !tooltipRoot.gameObject.activeSelf || _tooltipFollowTarget == null) return;
        SyncTooltipPosition();
    }

    void SyncTooltipPosition()
    {
        if (_tooltipFollowTarget == null || tooltipRoot == null) return;
        Canvas canvas = tooltipRoot.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            tooltipRoot.position = _tooltipFollowTarget.position + (Vector3)tooltipScreenOffset;
            return;
        }
        var canvasRect = canvas.transform as RectTransform;
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, _tooltipFollowTarget.position);
        screen += tooltipScreenOffset;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, screen, cam, out Vector3 world))
            tooltipRoot.position = world;
        else
            tooltipRoot.position = _tooltipFollowTarget.position + (Vector3)tooltipScreenOffset;
    }
}
