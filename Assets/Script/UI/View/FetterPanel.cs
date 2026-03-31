using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 那么问题来了 Fetter系统要不要放在这里面呢 我的倾向是不要 
/// </summary>
public class FetterPanel : View
{
    public GameObject fetterIcon;//这个Icon是用来显示羁绊效果的
    public RectTransform fetterIconParent;//放icon的地方

    [Header("羁绊效果说明（悬停/长按）")]
    [SerializeField] RectTransform tooltipRoot;
    [SerializeField] TMP_Text tooltipDescriptionText;
    [Tooltip("相对羁绊 Icon 中心的偏移（像素）")]
    [SerializeField] Vector2 tooltipScreenOffset = new Vector2(0f, 48f);

    List<GameObject> icons;
    RectTransform _tooltipFollowTarget;

    void Awake()
    {
        if (tooltipRoot != null)
            tooltipRoot.gameObject.SetActive(false);
    }

    public override void Init()
    {
        icons = new List<GameObject>();
    }
    public override void Show()
    {
        base.Show();
    }
    public void ShowFetter(Fetter fetter)
    {
        GameObject icon= Instantiate(fetterIcon, fetterIconParent);
        icon.GetComponent<FetterIcon>().ShowFetterIcon(fetter);
        icons.Add(icon);
    }
    public override void Hide()
    {
        HideFetterTooltip();
        base.Hide();
        for(int i = icons.Count - 1; i >= 0; i--)
        {
            Destroy(icons[i]);
        }
        icons.Clear();
    }

    /// <summary>由 <see cref="FetterIcon"/> 调用：显示说明并跟随指定 Icon。</summary>
    public void ShowFetterTooltip(RectTransform iconRect, string text)
    {
        if (tooltipRoot == null || tooltipDescriptionText == null || iconRect == null)
            return;
        _tooltipFollowTarget = iconRect;
        tooltipDescriptionText.text = text ?? "";
        tooltipRoot.gameObject.SetActive(true);
        SyncTooltipPosition();
    }

    /// <summary>由 <see cref="FetterIcon"/> 调用：关闭说明。</summary>
    public void HideFetterTooltip()
    {
        _tooltipFollowTarget = null;
        if (tooltipRoot != null)
            tooltipRoot.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (tooltipRoot == null || !tooltipRoot.gameObject.activeSelf || _tooltipFollowTarget == null)
            return;
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
