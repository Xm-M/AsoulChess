using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter_Weapon : LongDistance
{
    public string chessLayer;
    float realAttackRange;
    
    public override void InitController(Chess chess)
    {
        base.InitController(chess);
        
    }
    public override void WhenControllerEnterWar()
    {
        base.WhenControllerEnterWar();
        realAttackRange = master.propertyController.GetAttackRange() * MapManage.instance.tileSize.x;
        //Debug.Log(chess.targetLayer+" "+LayerMask.LayerToName(chess.targetLayer));
        if (CompareTag("Player"))
            chessLayer = "Enemy";

        else
            chessLayer = "Player";
    }
    public override bool IfInRange()
    {
        RaycastHit2D hit = Physics2D.Raycast(master.transform.position, master.transform.right, realAttackRange, LayerMask.GetMask(chessLayer));
        
        if (hit.collider != null)
        {
            target =hit.collider.GetComponent<Chess>();
            return true;
        }
        return false;
    }
    public override void Shoot()
    {
        base.Shoot();
    }
    public virtual void ShootBullet(Bullet bullet){
        if (target == null) return;
        Bullet b= ObjectPool.instance.Create(bullet.gameObject).GetComponent<Bullet>();
        b.transform.position = shootPos.position;
        b.InitBullet(master);
    }
}
