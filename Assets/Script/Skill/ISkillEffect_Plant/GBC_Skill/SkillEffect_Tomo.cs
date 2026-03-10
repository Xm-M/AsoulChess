using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// 
/// </summary>
public class SkillEffect_Tomo : ISkillEffect
{
    public Buff_Tomo_Correct buffTomo;
    IFindAll_Circle find;
    public DamageMessege DM;// buff也加在这里面 造成切割伤害  
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (find == null) find = new IFindAll_Circle();
        //寻找周围所有的单位并施加矫正buff
        find.FindTarget(user, targets);
        DM.damageFrom = user;
        foreach(var chess in targets)
        {
            DM.damageTo = chess;
            DM.damage = user.propertyController.GetAttack() * config.baseDamage[0];
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
    void EnsureBuffs()
    {
        if (attackBuff == null) attackBuff = new Buff_BaseValueBuff_Attack();
    }
    protected override void PrepareForRestore() => EnsureBuffs();
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
        timer = GameManage.instance.timerManage.AddTimer(AddStress, stressFrequence, true);
        base.BuffEffect(target);
    }
    public void AddStress()
    {
        int stress = 0;
        user.skillController.context.TryGet<int>("stress", out stress);
        stress += currentStress;
        user.skillController.context.Set<int>("stress", stress);
    }
    public override void BuffOver()
    {
        if (attackBuff != null) attackBuff.BuffOver();
        timer?.Stop();
        timer = null;
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
