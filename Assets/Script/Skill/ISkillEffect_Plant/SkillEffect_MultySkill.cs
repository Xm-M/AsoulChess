using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_MultySkill : ISkill
{
    [SerializeReference]
    public List<ISkill> skills;
    int current;
    public bool IfSkillReady(Chess user)
    {
        for(int i = 0; i < skills.Count; i++)
        {
            if (skills[i].IfSkillReady(user))
            {
                current = i;
                user.animatorController.ChangeFloat("skill", i);
                return true;
            }
        }
        return false;
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
        foreach(var skill in skills)
        {
            skill.LeaveSkill(user);    
        }
    }

    public void UseSkill(Chess user)
    {
        skills[current].UseSkill(user);
    }

    public void WhenEnter(Chess user)
    {
        foreach(var skill in skills)
        {
            skill.WhenEnter(user);
        }
    }
}
