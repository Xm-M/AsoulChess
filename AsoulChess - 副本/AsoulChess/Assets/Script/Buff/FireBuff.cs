using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="FireBuff",menuName ="Buff/FireBuff")]
public class FireBuff : Buff
{
    [SerializeField]protected float damage;
    [SerializeField] protected Color fire;
    
    //public override void BuffEffect(DamageMessege damageMessege)
    //{
    //    damageMessege.damage = damage;
    //    damageMessege.damageTo.GetDamage(damageMessege);
    //}
    //public override void BuffReset(DamageMessege target)
    //{
    //    base.BuffReset(target);
    //    target.damageTo.GetComponent<SpriteRenderer>().color = fire;
    //}
    //public override void BuffOver(DamageMessege target)
    //{
    //    base.BuffOver(target);
    //    target.damageTo.GetComponent<SpriteRenderer>().color = Color.white;
    //}
}
