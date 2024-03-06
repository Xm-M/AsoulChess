using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 
public class TimeExplode_TimeSkill : TimeSkill
{
    public float explodeRange;
    public LayerMask layer;
    public DamageMessege mes;
    //public override void SkillEffect(Chess user)
    //{
    //    base.SkillEffect(user);
        
    //    mes.damageFrom = user;
    //    Collider2D[] collider2s= Physics2D.OverlapCircleAll(user.transform.position,explodeRange,layer);
    //    for(int i = 0; i < collider2s.Length; i++)
    //    {
    //        if(!collider2s[i].CompareTag(user.tag))
    //            collider2s[i].GetComponent<Chess>().propertyController.GetDamage(mes);
    //    }
    //    //Debug.Log("explode");
    //    user.Death();
    //}
}
