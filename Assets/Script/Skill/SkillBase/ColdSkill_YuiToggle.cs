using UnityEngine;

/// <summary>
/// 切换类主动：进入 SkillState 时立刻结算技能效果（无需等 Spine/Animator 的 OnSkill 事件）。
/// 请在技能动画上不要重复绑定 UseSkill，否则会切换两次。
/// </summary>
public class ColdSkill_YuiToggle : ColdSkill, ISkillFireUseSkillOnEnter
{
    public void FireUseSkillOnEnter(Chess chess)
    {
        chess.UseSkill();
    }
}

/// <summary>由 <see cref="SkillState"/> 在播放技能动画后调用，用于「入场即放」的主动。</summary>
public interface ISkillFireUseSkillOnEnter
{
    void FireUseSkillOnEnter(Chess chess);
}
