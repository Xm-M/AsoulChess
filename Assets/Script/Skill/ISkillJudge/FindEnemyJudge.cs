using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindEnemyJudge : ISkillJudge
{
    [SerializeReference]
    [LabelText("Ñ°µÐ")]
    public IFindTarget findTarget;
    public bool IfSkillOver(Skill skill)
    {
        skill.targets.Clear();
        findTarget.FindTarget(skill.user, skill.targets);
        return skill.targets.Count > 0;
    }
}
//