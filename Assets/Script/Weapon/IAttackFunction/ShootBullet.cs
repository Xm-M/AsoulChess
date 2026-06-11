using Sirenix.OdinInspector;
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
        if (targets.Count != 0)
        {
            zidan.InitBullet(user, user.equipWeapon.weaponPos.position, targets[0], user.transform.right);
            zidan.Dm.damageTo = targets[0];
        }
        else
        {
            zidan.InitBullet(user, user.equipWeapon.weaponPos.position, null, user.transform.right);
        }
        
        //Debug.Log(zidan.Dm.damageTo.name);
    }
}

/// <summary>
/// 对索敌结果 <paramref name="targets"/> 中每个单位各发射一枚子弹（<see cref="ShootBullet"/> 仅对 <c>targets[0]</c> 单发）。
/// 用于多目标时一人一发。
/// </summary>
public class ShootBullet_ShootAllTarget : IAttackFunction
{
    public GameObject bullet;

    public void Attack(Chess user, List<Chess> targets)
    {
        if (user?.equipWeapon?.weaponPos == null || bullet == null || targets == null || targets.Count == 0)
            return;

        for (int i = 0; i < targets.Count; i++)
        {
            Chess t = targets[i];
            if (t == null || t.IfDeath)
                continue;

            GameObject b = ObjectPool.instance.Create(bullet);
            if (b == null)
                continue;
            Bullet zidan = b.GetComponent<Bullet>();
            if (zidan == null)
                continue;

            zidan.InitBullet(user, user.equipWeapon.weaponPos.position, t, user.transform.right);
            zidan.Dm.damageTo = t;
        }
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
 

 
public class ShootBullet_WaitBullet : IAttackFunction
{
    public GameObject bullet;
    public string pamareName = "wait";
    Bullet zidan;
    Chess user;
    public void Attack(Chess user, List<Chess> targets)
    {
        this.user = user;
        GameObject b = ObjectPool.instance.Create(bullet);
        user.animatorController.ChangeFloat(pamareName, 0);
        zidan = b.GetComponent<Bullet>();
        if (targets.Count == 0) return;
        zidan.InitBullet(user, user.equipWeapon.weaponPos.position, targets[0], user.transform.right);
        //zidan.targetPos =;
        zidan.Dm.damageTo = targets[0];
        user.StartCoroutine(WaitBullet());
    }
    IEnumerator WaitBullet()
    {
        while (zidan != null&&zidan.gameObject.activeSelf )
        {
            yield return null;  
        }
        user.animatorController.ChangeFloat(pamareName,1);
    }
}
