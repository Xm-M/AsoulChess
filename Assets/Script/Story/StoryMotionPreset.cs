using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 剧情位移手感预设：通用则多角色/多事件共用，特例则单独复制一份（同状态机 SO 习惯）。
/// 距离/落点不在此配置，由 Binding 的 Anchor 决定。
/// </summary>
[CreateAssetMenu(fileName = "StoryMotionPreset", menuName = "Story/Motion Preset")]
public class StoryMotionPreset : ScriptableObject
{
    [LabelText("类型")]
    public StoryMotionKind kind = StoryMotionKind.MoveTo;

    [LabelText("时长(秒)"), MinValue(0.01f)]
    public float duration = 0.45f;

    [LabelText("进度曲线"), Tooltip("0→1 水平/整体插值；为空则线性")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [LabelText("跳跃高度"), ShowIf("@kind == StoryMotionKind.JumpTo"), MinValue(0f)]
    public float peakHeight = 1.2f;

    [LabelText("高度曲线"), ShowIf("@kind == StoryMotionKind.JumpTo"),
     Tooltip("归一化 0→1→0；为空则用 4u(1-u) 抛物近似")]
    public AnimationCurve heightCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 1f),
        new Keyframe(1f, 0f));

    [LabelText("Knockback 距离"), ShowIf("@kind == StoryMotionKind.Knockback"), MinValue(0f)]
    public float knockbackDistance = 1f;

    [LabelText("可选 Animator 状态"), Tooltip("非空则 Play 时切到该状态（一层）")]
    public string animatorStateName;
}
