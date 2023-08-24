using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeBullet : SingleBullet 
{
    public override void InitBullet(Chess chess)
    {
        base.InitBullet(chess);
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
    }
    public override void RecycleBullet()
    {
        base.RecycleBullet();
    }
}
