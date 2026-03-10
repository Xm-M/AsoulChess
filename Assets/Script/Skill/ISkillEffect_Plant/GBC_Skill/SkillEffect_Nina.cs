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
/// 牛肉饭 Buff：Attack + AcceleRate + 事件（井芹仁菜特殊：改攻击距离+高攻）
/// </summary>
public class BeafRiceBuff : Buff
{
    [SerializeReference] public Buff_BaseValueBuff_Attack attackBuff;
    [SerializeReference] public Buff_BaseValueBuff_AcceleRate acceleRateBuff;
    [LabelText("修改攻击距离")] public float closeAttackRange = 3.75f;
    [UnityEngine.Serialization.FormerlySerializedAs("extraAttack")] public float _extraAttack = 0.1f;
    [UnityEngine.Serialization.FormerlySerializedAs("extraSpeed")] public float _extraSpeed = 0.25f;
    float baseAttackRange;
    public GameObject effect;
    public override void WriteExtraToSaveData(BuffSaveData data)
    {
        base.WriteExtraToSaveData(data);
        if (data == null) return;
        data.SetExtra("BaseAttackRange", baseAttackRange);
    }
    public override void RestoreExtraFromSaveData(BuffSaveData data)
    {
        base.RestoreExtraFromSaveData(data);
        if (data == null) return;
        baseAttackRange = data.GetExtraFloat("BaseAttackRange", 0);
    }
    void EnsureBuffs()
    {
        if (attackBuff == null) attackBuff = new Buff_BaseValueBuff_Attack { extraAttack = _extraAttack };
        if (acceleRateBuff == null) acceleRateBuff = new Buff_BaseValueBuff_AcceleRate { rate = _extraSpeed };
    }
    protected override void PrepareForRestore() => EnsureBuffs();
    public override void BuffEffect(Chess target)
    {
        EnsureBuffs();
        base.BuffEffect(target);
        if (target.propertyController.creator.chessName == "井芹仁菜")
        {
            if (baseAttackRange == 0) baseAttackRange = target.propertyController.GetAttackRange();
            target.propertyController.SetAtttackRange(closeAttackRange);
            target.skillController.context.Set<bool>("BeafRice", true);
            attackBuff.extraAttack = 1f;
            attackBuff.target = target;
            attackBuff.BuffEffect(target);
        }
        else
        {
            attackBuff.target = target;
            attackBuff.BuffEffect(target);
            acceleRateBuff.target = target;
            acceleRateBuff.BuffEffect(target);
        }
        if (effect != null) { var e = ObjectPool.instance.Create(effect); e.transform.position = target.transform.position; }
    }
    public override void BuffOver()
    {
        target.animatorController.ChangeFloat("type", 0);
        if (target.propertyController.creator.chessName == "井芹仁菜")
        {
            target.propertyController.SetAtttackRange(baseAttackRange);
            target.propertyController.ChangeAttack(-_extraAttack * 4f);
        }
        else
        {
            if (attackBuff != null) attackBuff.BuffOver();
            if (acceleRateBuff != null) acceleRateBuff.BuffOver();
        }
        base.BuffOver();
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        if (target.propertyController.creator.chessName == "井芹仁菜")
            target.skillController.context.Set<bool>("BeafRice", true);
    }
}