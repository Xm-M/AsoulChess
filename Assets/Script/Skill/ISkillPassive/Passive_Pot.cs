using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这个逻辑上有问题，以后会改
/// </summary>
public class Passive_Pot : ISkill
{
    public PotTile potTile;

    public bool IfSkillReady(Chess user)
    {
        throw new System.NotImplementedException();
    }

    //public void InitSkill(Skill skill)
    //{
    //    potTile.SetTile(skill.user.moveController.standTile);
    //}

    public void InitSkill(Chess user)
    {
        throw new System.NotImplementedException();
    }

    public void LeaveSkill(Chess user)
    {
        throw new System.NotImplementedException();
    }

    public void UseSkill(Chess user)
    {
        throw new System.NotImplementedException();
    }
}
