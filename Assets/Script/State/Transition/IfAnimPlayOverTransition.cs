using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这个名字真不能改 所以理解一下 这个就是技能结束的判断函数
/// </summary>
public class IfAnimPlayOverTransition : Transition
{
    public override bool ifReach(Chess chess)
    {
        return chess.skillController.activeSkill.IsSkillFinished(chess);
    }
    public override Transition Clone()
    {
        IfAnimPlayOverTransition clone = new IfAnimPlayOverTransition();
        return clone;
    }
}
public class IfAnimationOverTransisiton : Transition
{
    [Tooltip("Animator 第 0 层上的状态名（State 名，与 Animator 窗口里一致）。非空时：仅当该状态已播完（normalizedTime≥1 且不在 Transition）才为 true。留空则等同 AnimatorController.IfAnimPlayOver()。")]
    public string animName;

    public override bool ifReach(Chess chess)
    {
        var ac = chess.animatorController;
        var animator = ac?.animator;
        if (animator == null) return false;

        if (string.IsNullOrEmpty(animName))
            return ac.IfAnimPlayOver();

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName(animName))
            return false;

        // 与 IfAnimPlayOver 一致：播完且不在层内过渡（避免过渡边界误触发）
        return info.normalizedTime >= 1f && !animator.IsInTransition(0);
    }

    public override Transition Clone()
    {
        return new IfAnimationOverTransisiton
        {
            animName = this.animName
        };
    }
}