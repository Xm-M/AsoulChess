using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Sample : Weapon
{
    [SerializeReference]
    public IInitWeapon initWeapon;
    [SerializeReference]
    public IFindTarget  findTarget;
    [SerializeReference]
    public IAttackFunction attackFunction;
    public List<Chess> enemys;
    public float interval;
    public int FindEnemy(Chess user)
    {
        //throw new System.NotImplementedException();
        findTarget.FindTarget(user,enemys);
        return enemys.Count;
    }

    public float GetInterval()
    {
        //throw new System.NotImplementedException();
        return interval;
    }

    public void InitWeapon(AttackController attackController)
    {
         enemys= new List<Chess>();
         initWeapon?.InitWeapon(attackController);
    }

    public void TakeDamage(Chess user)
    {
        //throw new System.NotImplementedException();
        attackFunction.Attack(user, enemys);
    }
}//

