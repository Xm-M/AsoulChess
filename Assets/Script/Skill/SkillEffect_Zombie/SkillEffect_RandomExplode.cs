//using Sirenix.OdinInspector;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

///// <summary>
///// 重点是有没有这个盒子的问题
///// 爆炸的伤害类型都是在skillcontroller里使用的
///// </summary>
//public class SkillEffect_RandomExplode : ISkill
//{
//    [LabelText("爆炸概率")]
//    public float explodePos = 0.1f;
//    [SerializeReference]
//    [LabelText("寻敌方式")]
//    public IFindTarget findTarget;
//    [LabelText("爆炸伤害倍率")]
//    public float explodeRate=10;//一般都是攻击力180 爆炸伤害倍率为10
//    [LabelText("释放延迟")]
//    public float coldDown;//CD 
//    List<Chess> targets;
//    float mapsizex;
//    public bool IfSkillReady(Chess user)
//    {
//        float r=Random.Range(0, 1);
//        if(r < explodePos+(mapsizex-user.moveController.standTile.mapPos.x)*0.1f)
//        {
//            return true;
//        }
//        else
//        {
//            return false;
//        }
//    }

//    public void InitSkill(Chess user)
//    {
//        //throw new System.NotImplementedException();
//        mapsizex = MapManage_PVZ.instance.mapSize.x;
//    }

//    public void LeaveSkill(Chess user)
//    {
         
//    }
//    /// <summary>
//    /// 这里就是爆炸了 
//    /// </summary>
//    /// <param name="user"></param>
//    public void UseSkill(Chess user)
//    {
//        findTarget.FindTarget(user, targets);
//        for (int i = 0; i < targets.Count; i++)
//        {
//            user.skillController.DM.damageFrom = user;
//            user.skillController.DM.damageTo = targets[i];
//            user.skillController.DM.damageElementType = ElementType.Explode;
//            user.skillController.DM.damage =
//                user.propertyController.GetAttack() * explodeRate;
//            user.propertyController.TakeDamage(user.skillController.DM);
//        }
//    }
//    public void WhenEnter(Chess user)
//    {
         
//    }
//}
