using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 这个是soldier对应的weapon
/// 有几个信息是需要的 1.主要是改变的weapon方式 2.animtor对应的数值改变 
/// 这tm是个啥
/// </summary>
public class Passive_SoldierWeapon : ISkill
{
    [SerializeReference]
    public IAttackFunction weapon;
    [SerializeReference]
    public IFindTarget findTarget;
    public int id;
    public string property;
    Chess soldier;

    public bool IfSkillReady(Chess user)
    {
        throw new System.NotImplementedException();
    }

    public void InitSkill(Chess user)
    {
        soldier = user.moveController.standTile.stander;
        if (!soldier)
        {
            user.Death();
        }
        else
        {
            soldier.animator.SetInteger(property, id);
            //soldier.equipWeapon.ChangeWeapon(weapon,findTarget);
            user.Death();
        }
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
