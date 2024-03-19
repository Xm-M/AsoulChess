using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_Explode : ISkillEffect
{
    [SerializeReference]
    public IFindTarget findTarget;
    [SerializeReference]
    public IAttackFunction attack;
    public void SkillEffect(Skill skill)
    {
        findTarget.FindTarget(skill.user, skill.targets);
        attack.Attack(skill.user, skill.targets);
    }
}
