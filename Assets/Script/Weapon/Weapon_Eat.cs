using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Eat : Weapon
{
    public float interval;
    Chess user;
    [SerializeReference]
    public IFindTarget find;
    List<Chess> targets;
    public int FindEnemy(Chess user)
    {
        find.FindTarget(user, targets);
        return targets.Count;
    }

    public float GetInterval()
    {
        return interval;
    }

    public void InitWeapon(AttackController attackController)
    {
        targets = new List<Chess>();
        user = attackController.master;
    }

    public void TakeDamage(Chess user)
    {
         foreach (Chess target in targets)
        {
            target.Death();
        }
    }
}
