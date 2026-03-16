using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 效果是随机选择一个本行敌方单位(如果本行没有地方单位就直接跳到最后一列) 算了 先把跳到最后一列写出来
/// 先把技能做了 
/// </summary>
public class ISkillEffect_Zombie_BullyMan : ISkillEffect
{
    //public float JumpSpeed = 10f;
    public Buff_BaseValueBuff_UnSelectable unSelectableBuff;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        Debug.Log("使用技能");
        Tile targetTile = MapManage.instance.tiles[0, user.moveController.standTile.mapPos.y];
        Chess target = null;
        user.skillController.context.TryGet<Chess>("霸凌目标", out target);
        if (target != null) targetTile = target.moveController.standTile;
        float maxHeight = config.baseDamage[0];
        float movespeed = config.baseDamage[1];
        user.moveController.JumpToTarget(user,user.transform, user.transform.position, targetTile.transform.position, maxHeight, movespeed);
        if (unSelectableBuff == null) unSelectableBuff = new Buff_BaseValueBuff_UnSelectable();
        unSelectableBuff.continueTime = (Mathf.Abs(targetTile.transform.position.x-user.transform.position.x) / movespeed);
        //Debug.Log(unSelectableBuff.continueTime);
        user.buffController.AddBuff(unSelectableBuff);
        user.moveController.standTile  = targetTile;
    }
    
}
