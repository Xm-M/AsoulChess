using UnityEngine;

/// <summary>
/// 种植/进入 SkillState 时立刻结算主动效果（不等动画 UseSkill 事件）。
/// <see cref="SkillOver"/> 不自动 Death——灰烬型请在技能动画结束帧挂 Death 事件。
/// </summary>
public class ColdSkill_FireOnEnter : ColdSkill, ISkillFireUseSkillOnEnter
{
    public override void WhenEnter(Chess user)
    {
        user.UnSelectable();
        base.WhenEnter(user);
    }

    public void FireUseSkillOnEnter(Chess chess)
    {
        chess.UseSkill();
    }
}
