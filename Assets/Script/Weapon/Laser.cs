using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Shooter_Weapon
{

    public LineRenderer line;
    RaycastHit2D[] raycastHit2D;
    public GameObject hitEffect;
    public LayerMask layer;
    public override void Shoot()
    {
        line.enabled = true;
        line.SetPosition(0, shootPos.position);
        raycastHit2D = Physics2D.RaycastAll(shootPos.position,  shootPos.position-master.transform.position,10,layer);
        foreach(var a in raycastHit2D)
        {
            if (a.collider.GetComponent<Chess>()&&!a.collider.CompareTag(master.tag))
            {
                TakeDamage(a.collider.GetComponent<Chess>());
                line.SetPosition(1, (a.collider.transform.position ));
                ObjectPool.instance.Create(hitEffect).transform.position=a.collider.transform.position;
                return;
            }
        }
    }
    public void EndShoot()
    {
        line.SetPosition(0, Vector2.zero);
        line.SetPosition(1, Vector2.zero);
    }
}
