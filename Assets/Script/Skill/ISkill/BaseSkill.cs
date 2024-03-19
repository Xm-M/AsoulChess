using Sirenix.OdinInspector;
using UnityEngine;

public class BaseSkill : ISkill
{
    [SerializeReference]
    [LabelText("判断方式")]
    public ISkillJudge judge;
    [SerializeReference]
    [LabelText("技能效果")]
    public ISkillEffect attackFunction;
    [SerializeReference]
    [LabelText("初始化方式")]
    public ISkillPassive passive;
    public virtual bool ifSkillReady(Skill user)
    {
        if(judge == null)return true;
        else return judge.IfSkillOver(user);
    }

    public virtual void InitSkill(Skill chess)
    {
        passive?.InitSkill(chess);
    }

    public virtual void LeaveSkill(Skill user)
    {
        passive?.OverSkill(user);
    }
    public virtual void UseSkill(Skill user)
    {
        attackFunction?.SkillEffect(user);
    }
}
