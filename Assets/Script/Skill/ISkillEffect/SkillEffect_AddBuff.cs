using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_AddBuff : ISkillEffect
{
    [SerializeReference]
    public Buff addBuff;
    public void SkillEffect(Skill skill)
    {
        skill.user.buffController.AddBuff(addBuff);
    }
}
