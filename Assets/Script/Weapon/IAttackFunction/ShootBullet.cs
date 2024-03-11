using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 有几个target就发射几次
/// </summary>
public class ShootBullet : IAttackFunction
{
    public GameObject bullet;
    public void Attack(Chess user, List<Chess> targets)
    {
         GameObject b=ObjectPool.instance.Create(bullet);
        Bullet zidan=b.GetComponent<Bullet>();
        zidan.InitBullet(user, user.equipWeapon.weaponPos.position);
    }
}
