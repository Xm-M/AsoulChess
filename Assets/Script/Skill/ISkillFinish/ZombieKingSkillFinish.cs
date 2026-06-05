using UnityEngine;

/// <summary>
/// 僵王技能结束判定：蹦极为 enter→leave  Animator 串联时，仅在 <c>anim_bungee_1_leave</c> 播完才结束；
/// 其余招式仍用 <see cref="AnimatorController.IfAnimPlayOver"/>。
/// </summary>
public class ZombieKingSkillFinish : ISkillFinish
{
    public const string BungeeLeaveStateName = "anim_bungee_1_leave";

    public bool IsFinished(Chess user, SkillConfig config, SkillRuntimeInfo runtime)
    {
        if (user?.skillController?.context == null || user.animatorController?.animator == null)
            return true;

        if (user.skillController.context.TryGet<int>(ZombieKingContextKeys.SkillAnimKind, out int kindInt)
            && (ZombieKingSkillAnimKind)kindInt == ZombieKingSkillAnimKind.BungeeSummon)
            return IsBungeeLeaveAnimOver(user.animatorController.animator);

        return user.animatorController.IfAnimPlayOver();
    }

    static bool IsBungeeLeaveAnimOver(Animator animator)
    {
        if (animator == null) return true;
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName(BungeeLeaveStateName))
            return false;
        return info.normalizedTime >= 1f && !animator.IsInTransition(0);
    }
}
