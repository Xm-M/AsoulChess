using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �����soldier��Ӧ��weapon
/// �м�����Ϣ����Ҫ�� 1.��Ҫ�Ǹı��weapon��ʽ 2.animtor��Ӧ����ֵ�ı� 
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
