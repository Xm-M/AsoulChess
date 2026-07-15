using UnityEngine;

/// <summary>
/// 多首终局主动技结束判定：入场段 <c>skill</c> 播完即可离开 SkillState，
/// 勿卡在无限循环的 <c>skill_loop</c>。
/// 退出后外观由 Blend=3 idle 承担；普攻/主动技由 FinaleLocked + AttackAble=false 禁止。
/// </summary>
public class SkillFinish_MultiHeadGhostBurst : ISkillFinish
{
    public bool IsFinished(Chess user, SkillConfig config, SkillRuntimeInfo runtime)
    {
        Animator anim = user?.animatorController?.animator;
        if (anim == null)
            return true;

        if (!MultiHeadMutsumiKeys.IsFinaleLocked(user))
            return user.animatorController.IfAnimPlayOver();

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("skill"))
            return info.normalizedTime >= 0.99f && !anim.IsInTransition(0);

        return true;
    }
}
