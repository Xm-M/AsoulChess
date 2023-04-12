using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffBullet : Bullet
{
    public Buff bulletBuff;
    public override void WhenBulletTakeDamage(Chess c)
    {
        base.WhenBulletTakeDamage(c);
        c.buffController.AddBuff(shooter, bulletBuff);
    }
}
