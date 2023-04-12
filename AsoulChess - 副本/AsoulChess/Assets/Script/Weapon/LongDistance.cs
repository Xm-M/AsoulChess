using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongDistance : Weapon
{
    public GameObject bullet;
    public Transform shootPos;
    public virtual void Shoot()
    {
        if (master.target == null) return;
        Bullet b= ObjectPool.instance.Create(bullet).GetComponent<Bullet>();
        //b.target = master.target.transform;
        Vector2 targerPos = master.target.transform.position;
        //b.ShootTo(master.property.GetValue(ValueType.Attack).currentValue,targerPos,BulletEffectType.Single);
        b.shooter = this.master;
        b.transform.position = shootPos.transform.position;
        //b.transform.right = master.target.transform.position - shootPos.transform.position;
        
        
    }
}
