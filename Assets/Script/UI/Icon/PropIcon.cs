using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>道具栏图标：悬停/长按显示说明（由 <see cref="PropPanel"/> 绘制）。</summary>
public class PropIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Image propImage;
    public TMP_Text propNameText;

    [Min(0.05f)]
    [SerializeField]
    float longPressDuration = 0.45f;

    PropItemData _prop;
    RectTransform _rect;
    PropPanel _panel;
    bool _hovering;
    bool _pointerDown;
    bool _tooltipFromLongPress;
    Coroutine _longPressCo;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _panel = GetComponentInParent<PropPanel>();
        if (propImage != null)
            propImage.raycastTarget = true;
    }

    public void ShowPropIcon(PropItemData prop)
    {
        _prop = prop;
        if (prop == null) return;
        if (propImage != null)
            propImage.sprite = prop.icon;
        if (propNameText != null)
            propNameText.text = string.IsNullOrEmpty(prop.displayName) ? prop.GetPropId() : prop.displayName;
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
