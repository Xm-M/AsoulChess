using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>道具栏图标：悬停/长按显示说明（由 <see cref="PropPanel"/> 绘制）；可选点击回调（Run HUD 列表）。</summary>
public class PropIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public Image propImage;
    public TMP_Text propNameText;

    [Min(0.05f)]
    [SerializeField]
    float longPressDuration = 0.45f;

    /// <summary>非空时视为选卡列表模式：点击回调详情，悬停不弹 tooltip。</summary>
    public Action<PropItemData> onClicked;

    PropItemData _prop;
    RectTransform _rect;
    PropPanel _panel;
    bool _hovering;
    bool _pointerDown;
    bool _tooltipFromLongPress;
    Coroutine _longPressCo;

    Image _rootImage;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _panel = GetComponentInParent<PropPanel>();
        _rootImage = GetComponent<Image>();
        // 仅关掉「无 Sprite」的纯白底；若根节点已配卡背/框图则保留
        if (_rootImage != null && _rootImage != propImage && _rootImage.sprite == null)
            _rootImage.enabled = false;
        if (propImage != null)
            propImage.raycastTarget = true;
    }

    public void ShowPropIcon(PropItemData prop)
    {
        _prop = prop;
        if (prop == null) return;
        if (propImage != null)
        {
            propImage.sprite = prop.icon;
            propImage.enabled = prop.icon != null;
            propImage.preserveAspect = true;
        }
        if (propNameText != null)
            propNameText.text = string.IsNullOrEmpty(prop.displayName) ? prop.GetPropId() : prop.displayName;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (onClicked == null || _prop == null)
            return;
        onClicked.Invoke(_prop);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovering = true;
        _tooltipFromLongPress = false;
        TryShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovering = false;
        if (!_tooltipFromLongPress)
            TryHideTooltip();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pointerDown = true;
        if (_longPressCo != null)
            StopCoroutine(_longPressCo);
        _longPressCo = StartCoroutine(LongPressRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pointerDown = false;
        if (_longPressCo != null)
        {
            StopCoroutine(_longPressCo);
            _longPressCo = null;
        }
        if (_tooltipFromLongPress)
        {
            TryHideTooltip();
            _tooltipFromLongPress = false;
        }
    }

    IEnumerator LongPressRoutine()
    {
        yield return new WaitForSeconds(longPressDuration);
        if (!_pointerDown || _hovering) yield break;
        if (ResolvePanel() == null) yield break;
        _tooltipFromLongPress = true;
        TryShowTooltip();
    }

    PropPanel ResolvePanel()
    {
        if (_panel == null)
            _panel = GetComponentInParent<PropPanel>();
        return _panel;
    }

    void TryShowTooltip()
    {
        if (onClicked != null) return;
        if (_prop == null || string.IsNullOrEmpty(_prop.effectDescription)) return;
        var p = ResolvePanel();
        if (p == null) return;
        p.ShowPropTooltip(_rect, _prop.effectDescription);
    }

    void TryHideTooltip()
    {
        ResolvePanel()?.HidePropTooltip();
    }

    void OnDisable()
    {
        TryHideTooltip();
    }
}
