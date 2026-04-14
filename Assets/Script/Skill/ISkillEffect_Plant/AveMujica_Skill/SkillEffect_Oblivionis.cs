using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_Oblivionis : ISkillEffect
{
    public GameObject bullet;
     
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        GameObject b = ObjectPool.instance.Create(bullet);
        Bullet zidan = b.GetComponent<Bullet>();
        if (targets.Count != 0)
        {
            zidan.InitBullet(user, user.equipWeapon.weaponPos.position, targets[0], user.transform.right);
            zidan.Dm.damageTo = targets[0];
            zidan.rate = config.baseDamage[0];
        }
        else
        {
            zidan.InitBullet(user, user.equipWeapon.weaponPos.position, null, user.transform.right);
            zidan.rate = config.baseDamage[0];
        }

    }
}
