 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckReplace_M3 : ICheckReplace
{
    ReplaceSkill replace;
    Chess user;
    [SerializeField]int n = 0;//0是生成重构体技能，1是小猫哈气
    public string buffName;
    public void WhenEnter(Chess user, ReplaceSkill replaceSkill)
    {
        this.replace = replaceSkill;
        this.user = user;
        //EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), CheckCrystal);
        user.skillController.context.AddEvent(CheckCrystal);
    }
    public void WhenLeave(Chess user, ReplaceSkill replaceSkill)
    {
        //EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), Wait);
        user.skillController.context.RemoveEvent(CheckCrystal);
    }
    public void CheckCrystal()
    {
        Debug.Log("检查是否有重构体");
        Chess crystal=null;
        user.skillController.context.TryGet<Chess>(buffName,out crystal);
        Debug.Log(crystal);
        if (crystal != null&&n!=1)
        {
            n = 1;
            user.animatorController.ChangeInt(n);
            Debug.Log("有重构体 转换哈气");
            //replace.ChangeSkill(n);
            user.StartCoroutine(IEChange(replace, n));
        }
        else if(crystal==null&&n!=0)
        {
            n = 0;
            user.animatorController.ChangeInt(n);
            Debug.Log("没有重构体 转换为召唤");
            user.StartCoroutine(IEChange(replace, n));
            //replace.ChangeSkill(n); 这个蠢猪bug还在是我没想到的
        }
    }
    IEnumerator IEChange(ReplaceSkill replaceSkill,int n)
    {
        yield return null;
        replace.ChangeSkill(n);
    }
}
public class SkillEffect_Mon3tr : ISkillEffect
{
    [SerializeReference]
    public Buff_Create buff;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        user.buffController.AddBuff(buff);      
    }
}
/// <summary>
/// 哈基米这个技能到底要怎么写呢
/// 然后就是技能释放逻辑了 有点烦的是 它的实际效果要再检测一次效果就很烦了 
/// check->PlayLoop->idle->check 
/// 主要是如果没有idle的话 我可以少画一段idle动画
/// 重点是我自己也没有很想改啊 真的累
/// 现在的问题是返回的时候动画要怎么放
/// </summary>
public class SkillEffect_M3Rebuild : ISkillEffect
{
    Chess crystal;
    bool send = false;
    [SerializeReference]
    public IFindTarget findTarget;
    public string buffName;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        //第一步是传送到重构体
        if (!send)
        {
            if(crystal==null) user.skillController.context.TryGet<Chess>(buffName, out crystal);
            Debug.Log(crystal);
            Tile targetTile = crystal.moveController.standTile;
            Tile selfTile = user.moveController.standTile;
            targetTile.ChessLeave(crystal);
            selfTile.ChessLeave(user);
            targetTile.ChessEnter(user);
            targetTile.PlantChess(user);
            selfTile.ChessEnter(crystal);
            selfTile.PlantChess(crystal);
            send=true;
            user.skillController.onSkillOver.AddListener(GoBack);

        }
        else
        {
            //这里就是圆形检测 然后造成伤害了
            findTarget.FindTarget(user, targets);
            for (int i = 0; i < targets.Count; i++)
            {
                user.skillController.DM.damageFrom = user;
                user.skillController.DM.damageTo = targets[i];
                user.skillController.DM.damageElementType = ElementType.CloseAttack;
                user.skillController.DM.damageType = DamageType.Real;
                user.skillController.DM.damage =
                    user.propertyController.GetAttack() * config.baseDamage[0];
                user.propertyController.TakeDamage(user.skillController.DM);
            }
        }
    }


    /// <summary>
    /// 1不知道为什么 没有播放回来的动画
    /// 2不知道为什么 被种植过的格子不能再种植了
    /// </summary>
    /// <param name="chess"></param>
    public void GoBack(Chess chess)
    {
        send=false;
        chess.moveController.standTile.ChessLeave(chess);
        Tile targetTile = crystal.moveController.standTile;
        crystal.Death();
        targetTile.ChessEnter(chess);
        targetTile.PlantChess(chess);
        chess.skillController.onSkillOver.RemoveListener(GoBack);
    }
}