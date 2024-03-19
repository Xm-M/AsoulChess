using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive_Pot : ISkillPassive
{
    public PotTile potTile;
    public void InitSkill(Skill skill)
    {
        potTile.SetTile(skill.user.moveController.standTile);
    } 
    public void OverSkill(Skill skill)
    {

    }
}
