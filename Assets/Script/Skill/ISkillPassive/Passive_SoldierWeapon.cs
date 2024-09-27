using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �����soldier��Ӧ��weapon
/// �м�����Ϣ����Ҫ�� 1.��Ҫ�Ǹı��weapon��ʽ 2.animtor��Ӧ����ֵ�ı� 
/// ��tm�Ǹ�ɶ
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
