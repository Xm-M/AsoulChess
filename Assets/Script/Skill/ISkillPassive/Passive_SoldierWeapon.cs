using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 这个是soldier对应的weapon
/// 有几个信息是需要的 1.主要是改变的weapon方式 2.animtor对应的数值改变 
/// </summary>
public class Passive_SoldierWeapon : ISkillPassive
{
    [SerializeReference]
    public IAttackFunction weapon;
    [SerializeReference]
    public IFindTarget findTarget;
    Chess soldier;

    public void InitSkill(Skill user)
    {
        soldier = user.user.moveController.standTile.stander;
        if (!soldier)
        {
            user.user.Death();
        }
        else
        {
            soldier.equipWeapon.ChangeWeapon(weapon,findTarget);
        }
    }


    public void OverSkill(Skill user)
    {
        
    }
}
