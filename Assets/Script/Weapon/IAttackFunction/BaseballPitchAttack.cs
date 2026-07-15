using System;
using UnityEngine;

/// <summary>投手攻击：目标为挥棒手时发射喂球弹，否则发射普通子弹。</summary>
[Serializable]
public class BaseballPitchAttack : IAttackFunction
{
    public GameObject enemyBullet;
    public GameObject pitchBullet;

    public void Attack(Chess user, System.Collections.Generic.List<Chess> targets)
    {
        if (user?.equipWeapon?.weaponPos == null || targets == null || targets.Count == 0)
            return;

        Chess target = targets[0];
        if (target == null || target.IfDeath)
            return;

        if (BaseballRelayHelper.IsBatter(target))
        {
            SpawnFeedVisual(user, target);
            return;
        }

        if (enemyBullet == null)
            return;

        GameObject b = ObjectPool.instance.Create(enemyBullet);
        if (b == null)
            return;

        Bullet bullet = b.GetComponent<Bullet>();
        if (bullet == null)
            return;

        bullet.InitBullet(user, user.equipWeapon.weaponPos.position, target, user.transform.right);
        bullet.Dm.damageTo = target;
    }

    void SpawnFeedVisual(Chess user, Chess batter)
    {
        GameObject prefab = pitchBullet != null ? pitchBullet : enemyBullet;
        if (prefab == null)
            return;

        GameObject b = ObjectPool.instance.Create(prefab);
        if (b == null)
            return;

        if (b.GetComponent<Bullet_PitchBall>() is Bullet_PitchBall pitchBall)
        {
            pitchBall.InitBullet(user, user.equipWeapon.weaponPos.position, batter, user.transform.right, 0f, 1f);
            pitchBall.Dm.damageTo = batter;
            return;
        }

        if (b.GetComponent<Bullet>() is Bullet bullet)
        {
            bullet.InitBullet(user, user.equipWeapon.weaponPos.position, batter, user.transform.right, 0f, 1f);
            bullet.Dm.damageType = DamageType.Miss;
            bullet.Dm.damage = 0f;
            bullet.Dm.damageTo = batter;
        }
    }
}
