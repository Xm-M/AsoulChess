using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouthAttack : Weapon
{
    public override void Attack()
    {
        if (master.target == null) return;
        if (ifAttack == false)
        {
            GetComponent<Animator>().Play("attack");
            ifAttack = true;
        }
        transform.position = master.target.transform.position;
    }
    public override void TakeDamage(Chess target)
    {
        base.TakeDamage(target);
    }
    public virtual void Damage()
    {
        TakeDamage(master.target);
    }
}
