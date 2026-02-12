using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_FriendBuff : Bullet
{
    [SerializeReference]
    public Buff takeBuff;
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        Chess c=collision.GetComponent<Chess>();
        if (c != null&&c.CompareTag(tag))
        {
            c.buffController.AddBuff(takeBuff);
        }
    }
}
