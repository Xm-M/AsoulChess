//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
///// <summary>
///// 怎么没反应
///// </summary>
//public class Passive_MushRoom : ISkill
//{

//    public Color color;
//    public bool IfSkillReady(Chess user)
//    {
//        //throw new System.NotImplementedException();
//        return false;
//    }

//    public void InitSkill(Chess user)
//    {
//        //throw new System.NotImplementedException();
//    }

//    public void LeaveSkill(Chess user)
//    {
//        user.propertyController.onGetDamage.RemoveListener(MeiHuo);
//    }

//    public void UseSkill(Chess user)
//    {
//        user.propertyController.onGetDamage.AddListener(MeiHuo);
//    }
//    public void MeiHuo(DamageMessege dm)
//    {
//        //dm.damageFrom;
//        ChessTeamManage.Instance.ChangeTeam(dm.damageFrom);
//        dm.damageFrom.transform.right = -dm.damageFrom.transform.right;
//        dm.damageTo.Death();
//        dm.damageFrom.animatorController.ChangeColor(color);
//    }
//    public void WhenEnter(Chess user)
//    {
//        //throw new System.NotImplementedException();
//    }
//}
