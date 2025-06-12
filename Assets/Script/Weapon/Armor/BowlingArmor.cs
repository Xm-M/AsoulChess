using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlingArmor : CarArmor
{
    int prey=-1;
    public override void InitArmor()
    {
        base.InitArmor();
        prey = -1;
        user.WhenEnterGame.AddListener(ResetArmor);

    }
    public override void ResetArmor(Chess chess)
    {
        base.ResetArmor(chess);
        prey = -1;
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(user.tag))
        {
            Chess chess = collision.GetComponent<Chess>();
            if (chess.moveController.standTile.mapPos.y != prey)
            {
                dm.damageFrom = user;
                dm.damageTo = chess;
                dm.damage = user.propertyController.GetAttack();
                Debug.Log("Åö×²");
                onHit?.Invoke();
                user.propertyController.TakeDamage(dm);
                if (player != null)
                {
                    player.RandomPlay();
                }
                prey=chess.moveController.standTile.mapPos.y;
            }
        }
        
    }
}
