using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffBullet : Bullet
{
    public Buff bulletBuff;
    public override void InitBullet(Chess chess)
    {
        base.InitBullet(chess);
        chess.propertyController.onTakeDamage.AddListener(WhenTakeDamage);
    }
    public void WhenTakeDamage(DamageMessege mes)
    {
        mes.damageTo.buffController.AddBuff(mes.damageFrom,bulletBuff);
    }
     
}
