using UnityEngine;

/// <summary>
/// 司霆惊蛰 1 技能壳：
/// 进战保持不可选中；进入 <c>SkillState</c> 时恢复可选中；
/// 效果仍由动画 <c>UseSkill</c> 触发（本类的 <see cref="ISkillFireUseSkillOnEnter"/> 只恢复选中，不放技能）。
/// </summary>
public class ColdSkill_LeiziS1 : ColdSkill, ISkillFireUseSkillOnEnter
{
    public void FireUseSkillOnEnter(Chess chess)
    {
        if (chess != null)
            chess.ResumeSelectable();
    }

    public override void SkillOver(Chess user)
    {
        base.SkillOver(user);
        if (user != null && !user.IfDeath)
            user.UnSelectable();
    }
}
