using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 挂在 <see cref="StoryActor"/> 上：按 <see cref="StoryMotionPreset"/> 做公式化位移。
/// 某一时段位移由本组件或 Timeline 二选一，勿同时写 Position 轨。
/// </summary>
public class StoryMotionPlayer : MonoBehaviour
{
    [Tooltip("可选；Preset.animatorStateName 非空时使用")]
    public Animator animator;

    [Tooltip("localScale.x 为正时视为朝右")]
    public bool positiveScaleXFacesRight = true;

    Coroutine _running;
    Action _onComplete;

    public bool IsPlaying => _running != null;

    public void Play(StoryMotionPreset preset, Transform targetAnchor, Action onComplete)
    {
        if (preset == null)
        {
            onComplete?.Invoke();
            return;
        }

        Stop();
        _onComplete = onComplete;
        _running = StartCoroutine(RunMotion(preset, targetAnchor));
    }

    public void Stop()
    {
        if (_running != null)
        {
            StopCoroutine(_running);
            _running = null;
        }

        _onComplete = null;
    }

    IEnumerator RunMotion(StoryMotionPreset preset, Transform targetAnchor)
    {
        Vector3 start = transform.position;
        Vector3 end = ResolveEnd(preset, start, targetAnchor);
        FaceToward(end.x - start.x);

        if (animator != null && !string.IsNullOrEmpty(preset.animatorStateName))
            animator.Play(preset.animatorStateName, 0, 0f);

        float duration = Mathf.Max(0.01f, preset.duration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / duration);
            float e = EvaluateEase(preset, u);
            Vector3 pos = Vector3.LerpUnclamped(start, end, e);

            if (preset.kind == StoryMotionKind.JumpTo)
                pos.y = Mathf.LerpUnclamped(start.y, end.y, e) + EvaluateHeight(preset, u) * preset.peakHeight;

            transform.position = pos;
            yield return null;
        }

        transform.position = end;
        _running = null;
        var cb = _onComplete;
        _onComplete = null;
        cb?.Invoke();
    }

    Vector3 ResolveEnd(StoryMotionPreset preset, Vector3 start, Transform targetAnchor)
    {
        if (preset.kind == StoryMotionKind.Knockback)
        {
            if (targetAnchor != null)
                return targetAnchor.position;

            float face = transform.localScale.x;
            if (!positiveScaleXFacesRight)
                face = -face;
            float dir = face >= 0f ? -1f : 1f;
            return start + new Vector3(dir * preset.knockbackDistance, 0f, 0f);
        }

        return targetAnchor != null ? targetAnchor.position : start;
    }

    void FaceToward(float deltaX)
    {
        if (Mathf.Abs(deltaX) < 0.001f)
            return;

        var s = transform.localScale;
        float absX = Mathf.Abs(s.x) < 0.001f ? 1f : Mathf.Abs(s.x);
        bool wantPositive = deltaX > 0f;
        if (!positiveScaleXFacesRight)
            wantPositive = !wantPositive;
        s.x = wantPositive ? absX : -absX;
        transform.localScale = s;
    }

    static float EvaluateEase(StoryMotionPreset preset, float u)
    {
        if (preset.easeCurve != null && preset.easeCurve.length > 0)
            return preset.easeCurve.Evaluate(u);
        return u;
    }

    static float EvaluateHeight(StoryMotionPreset preset, float u)
    {
        if (preset.heightCurve != null && preset.heightCurve.length > 0)
            return preset.heightCurve.Evaluate(u);
        return 4f * u * (1f - u);
    }
}
