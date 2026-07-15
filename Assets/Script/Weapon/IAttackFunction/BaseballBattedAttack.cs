using System;
using UnityEngine;

/// <summary>挥棒手击球远程攻击，支持攻击力倍率。</summary>
[Serializable]
public class BaseballBattedAttack : IAttackFunction
{
    public GameObject bullet;
    public float damageRate = 1f;

    public void Attack(Chess user, System.Collections.Generic.List<Chess> targets)
    {
        if (user?.equipWeapon?.weaponPos == null || bullet == null || targets == null || targets.Count == 0)
            return;

        Chess target = targets[0];
        if (target == null || target.IfDeath)
            return;

        GameObject b = ObjectPool.instance.Create(bullet);
        if (b == null)
            return;

        Bullet zidan = b.GetComponent<Bullet>();
        if (zidan == null)
            return;

        zidan.InitBullet(user, user.equipWeapon.weaponPos.position, target, user.transform.right, -1f, damageRate);
        zidan.Dm.damageTo = target;
    }
}
