using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class CarArmor : ArmorBase
{
    public DamageMessege dm;
    public AudioPlayer player;
    public UnityEvent  onHit;
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
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(user.tag))
        {
            Chess chess =  collision.GetComponent<Chess>();
            dm.damageFrom = user;
            dm.damageTo = chess;
            dm.damage = user.propertyController.GetAttack();
            onHit?.Invoke( );
            user.propertyController.TakeDamage(dm);
            if(player != null)
            {
                player.RandomPlay();
            }
            
        }
    }
}
