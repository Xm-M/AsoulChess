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
        zidan.InitBullet(user, user.equipWeapon.weaponPos.position, targets[0].transform.position,user.transform.right);
    }
}
public class ShootBulletByDir : IAttackFunction
{
    public GameObject bullet;
    public Transform shooter;
    public void Attack(Chess user, List<Chess> targets)
    {
        GameObject b = ObjectPool.instance.Create(bullet);
        Bullet zidan = b.GetComponent<Bullet>();
        zidan.InitBullet(user, shooter.transform.position, targets[0].transform.position, shooter.transform.right);
    }
}
public class MultiAttack : IAttackFunction
{
    [SerializeReference]
    public List<IAttackFunction> attacks;
    public void Attack(Chess user, List<Chess> targets)
    {
        for(int i = 0; i < attacks.Count; i++)
        {
            attacks[i].Attack(user, targets);
        }
    }
}