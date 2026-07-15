using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI 悬停放大：鼠标进入时平滑放大 RectTransform，离开恢复。
/// 挂在与 <see cref="UnityEngine.UI.Button"/> 同一物体上（需有 raycast 目标）。
/// </summary>
[DisallowMultipleComponent]
public class UIHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] RectTransform scaleTarget;
    [SerializeField] float hoverScale = 1.08f;
    [SerializeField] float duration = 0.12f;
    [SerializeField] AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    Vector3 _normalScale = Vector3.one;
    Coroutine _anim;
    bool _enabled = true;

    void Awake()
    {
        scaleTarget ??= transform as RectTransform;
        CaptureNormalScale();
    }

    public void CaptureNormalScale()
    {
        if (scaleTarget != null)
            _normalScale = scaleTarget.localScale;
    }

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
        if (!enabled)
            AnimateTo(_normalScale);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_enabled)
            return;
        AnimateTo(_normalScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnimateTo(_normalScale);
    }

    void OnDisable()
    {
        if (_anim != null)
        {
            StopCoroutine(_anim);
            _anim = null;
        }

        if (scaleTarget != null)
            scaleTarget.localScale = _normalScale;
    }

    void AnimateTo(Vector3 target)
    {
        if (_anim != null)
            StopCoroutine(_anim);

        if (!isActiveAndEnabled || duration <= 0f || scaleTarget == null)
        {
            if (scaleTarget != null)
                scaleTarget.localScale = target;
            return;
        }

        _anim = StartCoroutine(AnimateRoutine(target));
    }

    IEnumerator AnimateRoutine(Vector3 target)
    {
        Vector3 from = scaleTarget.localScale;
        float t = 0f;
        float d = Mathf.Max(0.01f, duration);
        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            float u = curve != null && curve.length > 0
                ? curve.Evaluate(Mathf.Clamp01(t / d))
                : Mathf.Clamp01(t / d);
            scaleTarget.localScale = Vector3.LerpUnclamped(from, target, u);
            yield return null;
        }

        scaleTarget.localScale = target;
        _anim = null;
    }
}
