using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �м���target�ͷ��伸��
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
