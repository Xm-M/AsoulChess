using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 羁绊图标：悬停显示效果说明；无悬停时长按也会显示，松手关闭（触摸友好）。
/// 说明由父级 <see cref="FetterPanel"/> 显示，位置跟随本 Icon。
/// </summary>
public class FetterIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Image fetterImage;
    public TMP_Text fetterName;
    public TMP_Text fetterCount;

    [Min(0.05f)]
    [SerializeField]
    float longPressDuration = 0.45f;

    Fetter _fetter;
    RectTransform _rect;
    FetterPanel _panel;
    bool _hovering;
    bool _pointerDown;
    bool _tooltipFromLongPress;
    Coroutine _longPressCo;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _panel = GetComponentInParent<FetterPanel>();
        if (fetterImage != null)
            fetterImage.raycastTarget = true;
    }

    public void ShowFetterIcon(Fetter fetter)
    {
        _fetter = fetter;
        if (fetter == null) return;
        fetterImage.sprite = fetter.fetterIcon;
        fetterName.text = fetter.fetterName;
        fetterCount.text = fetter.num.ToString();
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
        if (!_pointerDown) yield break;
        if (_hovering) yield break;
        if (ResolvePanel() == null) yield break;
        _tooltipFromLongPress = true;
        TryShowTooltip();
    }

    FetterPanel ResolvePanel()
    {
        if (_panel == null)
            _panel = GetComponentInParent<FetterPanel>();
        return _panel;
    }

    void TryShowTooltip()
    {
        if (_fetter == null || string.IsNullOrEmpty(_fetter.fetterEffectDescription)) return;
        var p = ResolvePanel();
        if (p == null) return;
        p.ShowFetterTooltip(_rect, _fetter.fetterEffectDescription);
    }

    void TryHideTooltip()
    {
        var p = ResolvePanel();
        if (p == null) return;
        p.HideFetterTooltip();
    }

    void OnDisable()
    {
        TryHideTooltip();
    }
}
