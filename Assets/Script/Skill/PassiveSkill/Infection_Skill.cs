using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class Infection_Skill : PassiveSkill
{
    //public override void SkillEffect(Chess user)
    //{
    //    base.SkillEffect(user);
    //    Debug.Log(user.propertyController);
    //    user.propertyController.onTakeDamage.AddListener(AttackToInfection);
    //}
    //public void AttackToInfection(DamageMessege mes)
    //{
    //    if (mes.damageTo.propertyController.GetHp() <= 0)
    //    {
    //        ChessFactory.instance.ChessCreate(mes.damageFrom.propertyController.creator.chessPre,
    //            mes.damageTo.moveController.standTile,mes.damageFrom.tag);
    //        //Debug.Log("infection");
    //        mes.damageTo.Death();
    //    }
    //}
}
