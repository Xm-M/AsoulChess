using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenShoot : Weapon
{
    public GameObject greenb;
    public void GreenAttack()
    {
        GameObject g= ObjectPool.instance.Create(greenb);
        g.transform.position=master.target.standTile.transform.position;
        g.tag=master.tag;
    }
}
