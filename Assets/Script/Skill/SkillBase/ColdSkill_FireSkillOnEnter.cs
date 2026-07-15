using UnityEngine;

/// <summary>进入 SkillState 时立刻 UseSkill，不把棋子切 UnSelectable（多首等需保持可点选）。</summary>
public class ColdSkill_FireSkillOnEnter : ColdSkill, ISkillFireUseSkillOnEnter
{
    public void FireUseSkillOnEnter(Chess chess)
    {
        chess.UseSkill();
    }
}
