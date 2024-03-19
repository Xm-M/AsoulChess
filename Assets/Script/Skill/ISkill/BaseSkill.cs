using Sirenix.OdinInspector;
using UnityEngine;

public class BaseSkill : ISkill
{
    [SerializeReference]
    [LabelText("�жϷ�ʽ")]
    public ISkillJudge judge;
    [SerializeReference]
    [LabelText("����Ч��")]
    public ISkillEffect attackFunction;
    [SerializeReference]
    [LabelText("��ʼ����ʽ")]
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
