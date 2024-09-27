using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockBullet :Bullet
{
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!CompareTag(collision.tag))
        {
            Chess c = collision.GetComponent<Chess>();
            if (c == null||c!=lockTarget) return;
            Dm.damageTo = c;
            if (current > 0)
               shooter.propertyController.TakeDamage(Dm);
            current--;
            if (current == 0)
            {
                RecycleBullet();
            }
        }
    }//
}
