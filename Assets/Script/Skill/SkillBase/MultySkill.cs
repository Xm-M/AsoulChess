using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 额 这个怎么写 后面看一下好吧 除了这个应该还有一个选项型的技能设置
/// </summary>
public class MultySkill : ISkill
{
    [SerializeReference]
    public List<ISkill> skills;
    public string skilltag="skill";
    ISkill currentSkill;
    public  bool IfSkillReady(Chess user)
    {
        //throw new System.NotImplementedException();
        for (int i= 0; i < skills.Count;i++)
        {
            if (skills[i].IfSkillReady(user))
            {
                currentSkill = skills[i];
                user.animatorController.animator.SetInteger(skilltag,i);
                return true;
            }

        }
        return false;
    }
    public void UseSkill(Chess user)
    {
        currentSkill.UseSkill(user);
    }
    public  void SkillOver(Chess user)
    {
        currentSkill.SkillOver(user);
    }

    public void InitSkill(Chess user)
    {
        foreach(var skill in skills)
        {
            skill.InitSkill(user);

        }
    }

    public void LeaveSkill(Chess user)
    {
        foreach (var skill in skills)
        {
            skill.LeaveSkill(user);

        }
    }

    public void WhenEnter(Chess user)
    {
        foreach (var skill in skills)
        {
            skill.WhenEnter(user);

        }
    }

    public bool IsSkillFinished(Chess user)
    {
        return currentSkill.IsSkillFinished(user);
    }

    public SkillConfig GetSkillConfig()
    {
        return currentSkill.GetSkillConfig();
    }

    public void ReturnCD()
    {
        currentSkill.ReturnCD();
    }
}

