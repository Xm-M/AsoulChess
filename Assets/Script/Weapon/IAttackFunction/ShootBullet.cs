using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  
/// </summary>
public class ShootBullet : IAttackFunction
{
    public GameObject bullet;
    public void Attack(Chess user, List<Chess> targets)
    {
         GameObject b=ObjectPool.instance.Create(bullet);
        Bullet zidan=b.GetComponent<Bullet>();
        if (targets.Count == 0) return;
        zidan.InitBullet(user, user.equipWeapon.weaponPos.position, targets[0] ,user.transform.right);
        zidan.Dm.damageTo = targets[0];
        //Debug.Log(zidan.Dm.damageTo.name);
    }
}
public class ShootBulletByDir : IAttackFunction
{
    public GameObject bullet;
    public Transform shooter;
    public void Attack(Chess user, List<Chess> targets)
    {
        if ((targets.Count == 0)) return;
        GameObject b = ObjectPool.instance.Create(bullet);
        Bullet zidan = b.GetComponent<Bullet>();
        zidan.InitBullet(user, shooter.transform.position, targets[0] , shooter.transform.right);
        zidan.Dm.damageTo = targets[0];
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
public class ShootBullet_ËïÉÐÏã : IAttackFunction
{
    public GameObject bullet;
    int index = 0;
    public void Attack(Chess user, List<Chess> targets)
    {
        if (index < 3)
        {
            GameObject b = ObjectPool.instance.Create(bullet);
            Bullet zidan = b.GetComponent<Bullet>();
            zidan.InitBullet(user, user.equipWeapon.weaponPos.position, targets[0], user.transform.right);
        }
        else
        {
            index = 0;
        }
    }
}
public class ShootBullet_Tomorin : IAttackFunction
{
    public List<GameObject> bullets;
    public Transform shooter;
    public void Attack(Chess user, List<Chess> targets)
    {
        if ((targets.Count == 0)) return;
        int n = Random.Range(0, bullets.Count);
        GameObject b = ObjectPool.instance.Create(bullets[n]);
        Bullet zidan = b.GetComponent<Bullet>();
        zidan.InitBullet(user, shooter.transform.position, targets[0], shooter.transform.right);
        
        zidan.Dm.damageTo = targets[0];
    }
}
//public class ShootBullet_Taki : IAttackFunction
//{
//    public List<GameObject> bullets;
//    public Transform shooter;
//    public int n;
//    public void Attack(Chess user, List<Chess> targets)
//    {
//        if ((targets.Count == 0)) return;
//        //int n = Random.Range(0, bullets.Count);
//        GameObject b = ObjectPool.instance.Create(bullets[n]);
//        Bullet zidan = b.GetComponent<Bullet>();
//        zidan.InitBullet(user, shooter.transform.position, targets[0], shooter.transform.right);

//        zidan.Dm.damageTo = targets[0];
//    }
//}