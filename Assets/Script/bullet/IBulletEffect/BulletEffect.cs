using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEfect_Wine : IBulletEffect
{
    public GameObject wineZone;//¾Æ×Õ
    public void OnBulletHit(Bullet bullet)
    {
        GameObject wine = ObjectPool.instance.Create(wineZone);
        Tile tile = bullet.Dm.damageTo.moveController.standTile;
        //Tile nextTile= bullet.Dm.damageTo.moveController.standTile;
        wine.GetComponent<TileEffect>().EnterTile(tile);
    }

}
