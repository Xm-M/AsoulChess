using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAttackSkillEffect : ISkillEffect
{
    [SerializeReference]
    [LabelText("¹¥»÷")]
    public IAttackFunction attackFunction;
    public void SkillEffect(Skill skill)
    {
        attackFunction.Attack(skill.user, skill.targets);
    }
}
