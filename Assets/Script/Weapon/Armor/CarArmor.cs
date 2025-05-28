using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarArmor : ArmorBase
{
    public DamageMessege dm;
    public AudioPlayer player;
    public override void BrokenArmor()
    {
         
    }

    public override void GetDamage(DamageMessege dm)
    {
         
    }

    public override void InitArmor()
    {
         
    }

    public override void ResetArmor(Chess chess)
    {
         
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(user.tag))
        {
            Chess chess =  collision.GetComponent<Chess>();
            dm.damageFrom = user;
            dm.damageTo = chess;
            dm.damage = user.propertyController.GetAttack();
            user.propertyController.TakeDamage(dm);
            if(player != null)
            {
                player.RandomPlay();
            }
        }
    }
}
