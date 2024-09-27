using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEffect_Fire :IBulletEffect 
{
    public DamageMessege dm;
    public float damage;
    public float range;
    public void OnBulletHit(Bullet bullet)
    {
        //Debug.Log("fire");
        Vector2 pos = bullet.transform.position;
        Collider2D[] cols= CheckObjectPoolManage.GetColArray(100);
        LayerMask layerMask = GameManage.instance.chessTeamManage.GetEnemyLayer(bullet.shooter.gameObject);
        int num= Physics2D.OverlapCircleNonAlloc(pos, range, cols, layerMask);
        for (int i = 0; i < num; i++)
        {
            
            dm.damageTo = cols[i].GetComponent<Chess>();
            dm.damage = damage;
            Debug.Log(dm.damageTo.name + "◊∆…’…À∫¶" + dm.damage);
            bullet.shooter.propertyController.TakeDamage(dm);
            
        }
    }
}
