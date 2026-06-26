using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tomo 主动：范围切割伤害 + 施加 <see cref="Buff_Tomo_Correct"/>（攻击与定时压力）。
/// </summary>
public class SkillEffect_Tomo : ISkillEffect
{
    public Buff_Tomo_Correct buffTomo;
    IFindAll_Circle find;
    public DamageMessege DM;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (find == null) find = new IFindAll_Circle();
        find.FindTarget(user, targets);

        Buff correctTemplate = buffTomo != null ? buffTomo : DM?.takeBuff;

        foreach (var chess in targets)
        {
            if (chess == null || chess.IfDeath || chess == user)
                continue;

            DM.damageFrom = user;
            DM.damageTo = chess;
            DM.damage = user.propertyController.GetAttack() * config.baseDamage[0];
            DM.takeBuff = correctTemplate != null ? correctTemplate.Clone() : null;
            user.propertyController.TakeDamage(DM);
        }
    }
}

/// <summary>
/// 矫正 Buff：TimeBuff + Attack（随层数增加）+ 事件（定时加压力）
/// </summary>
public class Buff_Tomo_Correct : TimeBuff
{
    [SerializeReference] public Buff_BaseValueBuff_Attack attackBuff;
    [Tooltip("目标尚无「压力」Buff 时施加（如非 GBC 植物）；施加后永久保留，与 GBC 被动一致。")]
    [SerializeReference] public Buff_StressBuff_Death guestStressBuff;
    public float stressFrequence = 5;
    [UnityEngine.Serialization.FormerlySerializedAs("extraAttack")] public float _extraAttack = 0.1f;
    public int extraStress = 1;
    public int baseStress = 5;
    [UnityEngine.Serialization.FormerlySerializedAs("baseAttack")] public float _baseAttack = 0.5f;
    public int maxCount = 5;
    int index;
    float currentAttack;
    int currentStress;
    Chess user;
    Timer stressTimer;

    void EnsureBuffs()
    {
        if (attackBuff == null) attackBuff = new Buff_BaseValueBuff_Attack();
    }

    protected override void PrepareForRestore() => EnsureBuffs();

    public override Buff Clone()
    {
        var c = (Buff_Tomo_Correct)base.Clone();
        c.attackBuff = attackBuff != null ? (Buff_BaseValueBuff_Attack)attackBuff.Clone() : null;
        c.stressTimer = null;
        return c;
    }

    public override void BuffEffect(Chess target)
    {
        EnsureBuffs();
        user = target;
        index = 0;
        currentAttack = _baseAttack;
        currentStress = baseStress;
        attackBuff.extraAttack = currentAttack;
        attackBuff.target = target;
        attackBuff.BuffEffect(target);
        EnsureStressBuffIfMissing(target);
        base.BuffEffect(target);
        if (stressFrequence > 0f)
            stressTimer = GameManage.instance.timerManage.AddTimer(AddStress, stressFrequence, true);
    }

    void EnsureStressBuffIfMissing(Chess target)
    {
        if (target?.buffController == null || target.buffController.buffDic.ContainsKey("压力"))
            return;

        Buff_StressBuff_Death template = guestStressBuff != null
            ? (Buff_StressBuff_Death)guestStressBuff.Clone()
            : new Buff_StressBuff_Death();
        target.buffController.AddBuff(template);
    }

    public void AddStress()
    {
        if (user == null)
            return;
        ApplyStressDelta(user, currentStress);
    }

    /// <summary>优先走「压力」Buff 的 <see cref="Buff_StressBuff_Death.BuffReset"/>，与 GBC 被动显示/阈值一致。</summary>
    static void ApplyStressDelta(Chess chess, int delta)
    {
        if (delta <= 0 || chess?.skillController?.context == null)
            return;

        if (chess.buffController?.buffDic == null
            || !chess.buffController.buffDic.TryGetValue("压力", out Buff buff)
            || buff is not Buff_StressBuff_Death stressBuff)
            return;

        stressBuff.BuffReset(new Buff_StressBuff_Death { extraStress = delta });
    }

    public override void BuffOver()
    {
        if (stressTimer != null)
        {
            stressTimer.Stop();
            stressTimer = null;
        }
        if (attackBuff != null) attackBuff.BuffOver();
        base.BuffOver();
    }

    public override int GetStackCount() => index;
    public override void SetStackCount(int v) => index = v;

    public override void WriteExtraToSaveData(BuffSaveData data)
    {
        base.WriteExtraToSaveData(data);
        if (data == null) return;
        data.SetExtra("CurrentAttack", currentAttack);
        data.SetExtra("CurrentStress", currentStress);
    }

    public override void RestoreExtraFromSaveData(BuffSaveData data)
    {
        base.RestoreExtraFromSaveData(data);
        if (data == null) return;
        currentAttack = data.GetExtraFloat("CurrentAttack", _baseAttack);
        currentStress = data.GetExtraInt("CurrentStress", baseStress);
        if (attackBuff != null) attackBuff.extraAttack = currentAttack;
    }

    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        index += 1;
        if (index < maxCount)
        {
            currentAttack += _extraAttack;
            currentStress += extraStress;
            if (attackBuff != null)
            {
                attackBuff.BuffOver();
                attackBuff.extraAttack = currentAttack;
                attackBuff.BuffEffect(target);
            }
        }
    }
}
