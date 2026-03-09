using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
/// <summary>
/// 这个是CD冷却 但是触发器是被攻击
/// 技能持续时间内都有闪避加成
/// 任何使用携程的技能都该被取代
/// </summary>
public class SkillEffect_Nina : ISkillEffect
{
    [LabelText("眩晕范围")]
    public float radiu;
    [LabelText("眩晕Buff")]
    [SerializeReference]
    public DizznessBuff dizznessBuff;
    [LabelText("闪避Buff")]
    [SerializeReference]
    public Buff_BaseValueBuff_DodgeRate dodgeBuff;
    /// <summary>
    /// 获得攻击免疫 
    /// </summary>
    /// <param name="user"></param>
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        user.buffController.AddBuff(dodgeBuff);
        user.skillController.onSkillOver.AddListener(OnSkillOver);
    }
    public void OnSkillOver(Chess user)
    {
        Collider2D[] cols = CheckObjectPoolManage.GetColArray(100);
        int n = Physics2D.OverlapCircleNonAlloc(user.transform.position, radiu, cols, ChessTeamManage.Instance.GetEnemyLayer(user.gameObject));
        for (int i = 0; i < n; i++)
        {
            Chess enemy = cols[i].GetComponent<Chess>();
            //Debug.Log(enemy.name + "眩晕");
            //所以到底要不要造成伤害 
            enemy.buffController.AddBuff(dizznessBuff);
        }
        CheckObjectPoolManage.ReleaseColArray(100, cols);
        user.buffController.TryOverBuff(dodgeBuff);
        user.skillController.onSkillOver.RemoveListener(OnSkillOver);
    }
}


/// <summary>
/// 主动技能
/// </summary>
public class SkillEffect_NinaTaunt : ISkillEffect
{
    [LabelText("嘲讽范围")]
    public float tauntRange=8.75f;
    [SerializeReference]
    [LabelText("生气Buff")]
    public AngryBuff angryBuff;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        RaycastHit2D[] hits = CheckObjectPoolManage.GetHitArray(100);
        int n = Physics2D.RaycastNonAlloc(user.transform.position, user.transform.right, hits, tauntRange, ChessTeamManage.Instance.GetEnemyLayer(user.gameObject));
        for(int i = 0; i < n; i++)
        {
            Chess chess= hits[i].collider.GetComponent<Chess>();
            chess.buffController.AddBuff(angryBuff);
            
        }
        CheckObjectPoolManage.ReleaseArray(100, hits);
    }
}

 

/// <summary>
/// 吉野家的牛肉饭 吃完后加20%攻击力 如果目标是小孩姐 则切换成
/// 有谁能告诉我为什么Nina的这个牛肉饭要搞的这么复杂吗
/// </summary>
public class BeafRiceBuff : Buff
{
    [LabelText("额外百分比攻击力")]
    public float extraAttack = 0.1f;
    [LabelText("修改攻击距离")]
    public float closeAttackRange = 3.75f;
    [LabelText("额外攻速")]
    public float extraSpeed = 0.25f;
    float baseAttackRange;
    public GameObject effect;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        if (target.propertyController.creator.chessName == "井芹仁菜")
        {
            target.propertyController.SetAtttackRange(closeAttackRange);
            target.skillController.context.Set<bool>("BeafRice", true);
            target.propertyController.ChangeAttack(1);
        }
        else
        {
            target.propertyController.ChangeAttack(extraAttack);
        }
        if (effect != null)
        {
            GameObject newEffect = ObjectPool.instance.Create(effect);
            newEffect.transform.position = target.transform.position;
        }
    }
 

    public override void BuffOver()
    {
        base.BuffOver();
        target.animatorController.ChangeFloat("type", 0);
        target.propertyController.ChangeAttack(-extraAttack);
        if (target.propertyController.creator.name == "井芹仁菜")
        {
            target.propertyController.SetAtttackRange(baseAttackRange);
            target.propertyController.ChangeAttack(-extraAttack * 4f);
        }
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        if (target.propertyController.creator.chessName == "井芹仁菜")
        {
            
            target.skillController.context.Set<bool>("BeafRice", true);
             
        }
    }
}