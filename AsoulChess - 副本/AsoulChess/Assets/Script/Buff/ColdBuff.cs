using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColdBuff", menuName = "Buff/ColdBuff")]
public class ColdBuff : Buff
{
    [SerializeField] private float speedreduce=0.5f;
    //public override void BuffReset(DamageMessege target)
    //{
    //    base.BuffReset(target);
    //    target.damageTo.property.SetCurrentValue(ValueType.Speed, -speedreduce);
    //    target.damageTo.GetComponent<SpriteRenderer>().color = Color.blue;
    //}
    //public override void BuffOver(DamageMessege target)
    //{
    //    base.BuffOver(target);
    //    Debug.Log("resetbuff");
    //    target.damageTo.property.SetCurrentValue(ValueType.Speed, speedreduce);
    //    target.damageTo.GetComponent<SpriteRenderer>().color = Color.white;
    //}
}
