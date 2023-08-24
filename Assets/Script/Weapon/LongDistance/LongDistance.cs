using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongDistance : Weapon
{
    public GameObject bullet;
    public Transform shootPos;
    public virtual void Shoot()
    {
        if (target == null) return;
        Bullet b= ObjectPool.instance.Create(bullet).GetComponent<Bullet>();
        b.transform.position = shootPos.position;
        b.InitBullet(master);
    }
}
