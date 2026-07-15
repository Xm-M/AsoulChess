using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 肉鸽 HUD 图标悬停说明：鼠标悬停 <see cref="hoverDelaySeconds"/> 秒后弹出说明（对标 <see cref="FetterIcon"/> + <see cref="FetterPanel"/>）。
/// 触摸设备无悬停时用同等时长长按。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class RoguelikeRunInfoHudTooltipTrigger : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] RoguelikeRunInfoHudStatKind statKind;
    [SerializeField] float hoverDelaySeconds = 2f;

    RoguelikeRunInfoPanel _panel;
    RectTransform _rect;
    bool _hovering;
    bool _pointerDown;
    bool _tooltipFromLongPress;
    Coroutine _delayCo;

    public void Configure(RoguelikeRunInfoPanel panel, RoguelikeRunInfoHudStatKind kind, float delaySeconds)
    {
        _panel = panel;
        statKind = kind;
        hoverDelaySeconds = Mathf.Max(0.05f, delaySeconds);
    }

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        if (_panel == null)
            _panel = GetComponentInParent<RoguelikeRunInfoPanel>();

        var image = GetComponent<Image>();
        if (image != null)
            image.raycastTarget = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovering = true;
        _tooltipFromLongPress = false;
        RestartDelayCoroutine();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovering = false;
        CancelDelayCoroutine();
        if (!_tooltipFromLongPress)
            TryHideTooltip();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pointerDown = true;
        CancelDelayCoroutine();
        _delayCo = StartCoroutine(LongPressRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pointerDown = false;
        CancelDelayCoroutine();
        if (_tooltipFromLongPress)
        {
            TryHideTooltip();
            _tooltipFromLongPress = false;
        }
    }

    IEnumerator LongPressRoutine()
    {
        yield return new WaitForSeconds(hoverDelaySeconds);
        if (!_pointerDown || _hovering)
            yield break;

        _tooltipFromLongPress = true;
        TryShowTooltip();
    }

    void RestartDelayCoroutine()
    {
        CancelDelayCoroutine();
        _delayCo = StartCoroutine(HoverDelayRoutine());
    }

    IEnumerator HoverDelayRoutine()
    {
        yield return new WaitForSeconds(hoverDelaySeconds);
        if (!_hovering)
            yield break;
        TryShowTooltip();
    }

    void CancelDelayCoroutine()
    {
        if (_delayCo == null)
            return;
        StopCoroutine(_delayCo);
        _delayCo = null;
    }

    RoguelikeRunInfoPanel ResolvePanel()
    {
        if (_panel == null)
            _panel = GetComponentInParent<RoguelikeRunInfoPanel>();
        return _panel;
    }

    void TryShowTooltip()
    {
        var panel = ResolvePanel();
        if (panel == null || _rect == null)
            return;

        string text = panel.GetHudStatTooltipText(statKind);
        if (string.IsNullOrEmpty(text))
            return;

        panel.ShowHudStatTooltip(_rect, text);
    }

    void TryHideTooltip()
    {
        ResolvePanel()?.HideHudStatTooltip();
    }

    void OnDisable()
    {
        CancelDelayCoroutine();
        TryHideTooltip();
    }
}
