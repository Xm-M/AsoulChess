using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseRange : Weapon
{
    public float range;
    public LayerMask layer;
    Collider2D[] collider2Ds;
    public override void InitController(Chess chess)
    {
        base.InitController(chess);
        collider2Ds = new Collider2D[11];
    }
    public override bool IfInRange()
    {
        Physics2D.OverlapCircleNonAlloc(transform.position, range,collider2Ds,layer);
        foreach(var collider in collider2Ds)
        {
            if (!collider.CompareTag(master.tag)) return true;
        }
        return false;
    }
}
