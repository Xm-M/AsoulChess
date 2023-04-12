using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerWood : Weapon
{
    public override void Attack()
    {
        base.Attack();
    }
    public void Effect()
    {
        //ObjectPool.instance.Create(effect).transform.position = master.target.transform.position;
    }
     
}
