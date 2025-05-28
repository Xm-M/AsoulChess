using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 锁定目标的子弹
/// </summary>
public class Bullet_Aming : Bullet
{
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (Dm.damageTo!=null&&Dm.damageTo.gameObject == collision.gameObject)
        {
            Chess c = collision.GetComponent<Chess>();
            if (c != null)
            {
                shooter.propertyController.TakeDamage(Dm);
                WhenBulletHit?.Invoke(this);
                effect?.OnBulletHit(this);
                RecycleBullet();
            }
        }else if (Dm.damageTo == null)
        {
            base.OnTriggerEnter2D(collision);
        }
    }
}
