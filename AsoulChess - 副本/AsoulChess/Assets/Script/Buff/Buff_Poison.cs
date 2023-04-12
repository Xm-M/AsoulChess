using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Buff_Poison", menuName = "Buff/Buff_Poison")]
public class Buff_Poison : Buff
{
    [SerializeField] protected float damage;
    [SerializeField] protected Color poison;

    //public override void BuffEffect(DamageMessege damageMessege)
    //{
    //    damageMessege.damage = damage;
    //    damageMessege.damageTo.GetDamage(damageMessege);
    //}
    //public override void BuffReset(DamageMessege target)
    //{
    //    base.BuffReset(target);
    //    target.damageTo.GetComponent<SpriteRenderer>().color = poison;
    //}
    //public override void BuffOver(DamageMessege target)
    //{
    //    base.BuffOver(target);
    //    target.damageTo.GetComponent<SpriteRenderer>().color = Color.white;
    //}
}
