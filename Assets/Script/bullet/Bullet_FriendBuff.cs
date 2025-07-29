using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_FriendBuff : Bullet
{
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        Chess c=collision.GetComponent<Chess>();
        if (c != null)
        {
            c.buffController.AddBuff(Dm.takeBuff);
        }
    }
}
