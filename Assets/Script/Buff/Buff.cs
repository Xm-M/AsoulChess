using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// Buff 分为两类：
/// 1. 基础数值 Buff (Buff_BaseValueBuff)：仅修改属性数值，如攻击力、攻速、减伤等
/// 2. 具体效果 Buff：有额外效果，通常注册事件、特效等，可通过组合多个 Buff_BaseValueBuff 实现
/// 例如：愤怒 Buff = TimeBuff + Buff_BaseValueBuff_AttackSpeed + Buff_BaseValueBuff_ExtraDefence(负值)
/// </summary>
#region 基础框架buff
[Serializable]
public abstract class Buff
{
    [LabelText("Buff名")]
    public string buffName;//这个buff的名字
    [HideInInspector] public Chess target;//buff的作用对象
    [System.NonSerialized] protected float _restoreRemainingTime = -1f;

    public virtual void WriteToSaveData(BuffSaveData data)
    {
        if (data == null) return;
        data.id = buffName;
        data.buffType = GetType().Name;
        CollectValueBuffsTo(data);
        WriteExtraToSaveData(data);
    }
    public virtual void RestoreFromSaveData(BuffSaveData data)
    {
        if (data == null) return;
        PrepareForRestore();
        if (data.remainingTime >= 0) _restoreRemainingTime = data.remainingTime;
        ApplyValueBuffsFrom(data);
        RestoreExtraFromSaveData(data);
    }
    /// <summary>读档前调用，确保子 Buff 已创建（如 EnsureBuffs）</summary>
    protected virtual void PrepareForRestore() { }
    /// <summary>写入额外数据（层数、currentAttack、count 等），子类重写</summary>
    public virtual void WriteExtraToSaveData(BuffSaveData data)
    {
        if (data == null) return;
        int sc = GetStackCount();
        if (sc != 1) data.SetExtra("StackCount", sc);
    }
    /// <summary>从 data 恢复额外数据，子类重写</summary>
    public virtual void RestoreExtraFromSaveData(BuffSaveData data)
    {
        if (data == null) return;
        float sc = (data.extraKeys != null && data.extraKeys.Contains("StackCount"))
            ? data.GetExtraFloat("StackCount", 1) : data.stackCount;
        SetStackCount((int)sc);
    }
    public virtual int GetStackCount() => 1;
    public virtual void SetStackCount(int v) { }
    /// <summary>收集子 Buff_BaseValueBuff 的数值到 data</summary>
    protected void CollectValueBuffsTo(BuffSaveData data)
    {
        if (data == null) return;
        var t = GetType();
        foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (f.IsStatic || f.IsLiteral) continue;
            try
            {
                if (f.GetValue(this) is Buff_BaseValueBuff vb)
                    data.SetValue(vb.GetType().Name, vb.GetSaveValue());
            }
            catch { }
        }
    }
    /// <summary>从 data 恢复子 Buff_BaseValueBuff 的数值，无 key 时保留原值</summary>
    protected void ApplyValueBuffsFrom(BuffSaveData data)
    {
        if (data == null) return;
        var t = GetType();
        foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (f.IsStatic || f.IsLiteral) continue;
            try
            {
                if (f.GetValue(this) is Buff_BaseValueBuff vb)
                {
                    float def = vb.GetSaveValue();
                    float v = data.GetValueFloat(vb.GetType().Name, def);
                    vb.SetSaveValue(v);
                }
            }
            catch { }
        }
    }

    public virtual void BuffReset(Buff resetBuff)
    {
    }

    public virtual void BuffEffect(Chess target)
    {
        this.target = target;
    }
    public virtual void BuffOver()
    {
        target.buffController.RemoveBuff(this);
    }
    public virtual Buff Clone()
    {
        return (Buff)this.MemberwiseClone();
    }
}
public class TimeBuff : Buff
{
    public float continueTime;
    protected Timer timer;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        float duration = _restoreRemainingTime >= 0 ? _restoreRemainingTime : continueTime;
        _restoreRemainingTime = -1f;
        timer = GameManage.instance.timerManage.AddTimer(BuffOver, duration, false);
    }
    public override void WriteToSaveData(BuffSaveData data)
    {
        base.WriteToSaveData(data);
        if (data != null && timer != null) data.remainingTime = timer.LeftTime();
    }
    public override void RestoreFromSaveData(BuffSaveData data)
    {
        base.RestoreFromSaveData(data);
        if (data != null && data.remainingTime >= 0 && timer != null) timer.ResetTime(data.remainingTime);
    }
    public override void BuffReset(Buff resetBuff)
    {
        continueTime = (resetBuff as TimeBuff).continueTime;
        //Debug.Log(continueTime);
        timer.ResetTime(continueTime);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        timer?.Stop();
        timer = null;
    }
}
public class MultyBuff : Buff
{
    [SerializeReference]
    public List<Buff> buffs;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        foreach(var buff in buffs){
            target.buffController.AddBuff(buff);
        }
        BuffOver();
    }

}
#endregion
#region 元素属性buff
/// <summary>
/// 冰冻 Buff：TimeBuff + 减速（AcceleRate）+ 冷气特效 + 变色
/// </summary>
public class ColdBuff : TimeBuff
{
    [LabelText("冷气特效")]
    public GameObject coldBuff;
    [SerializeReference] public Buff_BaseValueBuff_AcceleRate acceleRateBuff;
    [UnityEngine.Serialization.FormerlySerializedAs("slowRate")] public float _slowRate = -0.5f;
    public ColdBuff() { buffName = "冰冻"; }
    void EnsureBuffs()
    {
        if (acceleRateBuff == null) acceleRateBuff = new Buff_BaseValueBuff_AcceleRate { rate = _slowRate };
    }
    protected override void PrepareForRestore() => EnsureBuffs();
    public override void BuffEffect(Chess target)
    {
        EnsureBuffs();
        base.BuffEffect(target);
        acceleRateBuff.target = target; acceleRateBuff.BuffEffect(target);
        if (coldBuff != null) { var cold = ObjectPool.instance.Create(coldBuff); cold.transform.position = target.transform.position; }
        target.animatorController.ChangeColor(Color.blue);
    }
    public override void BuffOver()
    {
        if (acceleRateBuff != null) acceleRateBuff.BuffOver();
        target.animatorController.ChangeColor(Color.white);
        base.BuffOver();
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        var other = resetBuff as ColdBuff;
        if (other?.acceleRateBuff != null && acceleRateBuff != null) acceleRateBuff.BuffReset(other.acceleRateBuff);
    }
}
public class FireBuff : Buff
{
    public float damage = 13;//固定值 而且会被均摊
    public FireBuff()
    {
        buffName = "灼烧";
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        
    }
}
public class LightBuff : Buff
{
    [LabelText("光照范围")]
    public float range;
    public float hideTime;

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        if(Effect_Smoke.Instance!=null)
        Effect_Smoke.Instance.HideSmoke(target.transform.position, range, hideTime);
        BuffOver();
    }
    public LightBuff()
    {
        buffName = "光照";
    }
}

#endregion 
#region 控制buff
/// <summary>
/// 恐惧 Buff：转向并向后移动直到 buff 结束
/// </summary>
public class Buff_Fear : TimeBuff {

    [LabelText("恐惧特效")]
    public GameObject FearEffect;
    [LabelText("缓速效率")]
    public float moveRate = 0.5f;
    [SerializeReference]
    public Buff_BaseValueBuff_AcceleRate speedDownBuff;
    GameObject effect;
    StateName preState;

    void EnsureSpeedBuff()
    {
        if (speedDownBuff == null)
        {
            speedDownBuff = new Buff_BaseValueBuff_AcceleRate { rate = moveRate - 1f };
        }
    }

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        preState = target.stateController.currentState.state.stateName;

        effect = ObjectPool.instance.Create(FearEffect);
        effect.transform.SetParent(target.transform);
        effect.transform.localPosition = Vector3.zero;

        target.moveController.Turn();
        if (target.moveController.tileMethod != null && target.moveController.standTile != null && target.propertyController.GetMoveSpeed() > 0)
        {
            if (moveRate < 1f)
            {
                EnsureSpeedBuff();
                speedDownBuff.target = target;
                speedDownBuff.BuffEffect(target);
            }
            target.stateController.ChangeState(StateName.MoveState);
        }
    }

    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        if (resetBuff is Buff_Fear f) moveRate = f.moveRate;
    }

    public override void BuffOver()
    {
        if (moveRate < 1f && speedDownBuff != null)
            speedDownBuff?.BuffOver();
        ObjectPool.instance.Recycle(effect);
        target.moveController.StopMove();
        target.moveController.Turn();
        target.stateController.ChangeState(preState);
        base.BuffOver();
    }
}




/// <summary>
/// 魅惑buff
/// </summary>
public class Buff_Charm : Buff
{
    public Color color;
    public GameObject charmEffect;//魅惑特效
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        ChessTeamManage.Instance.ChangeTeam(target);
        target.transform.right = -target.transform.right;
        //dm.damageTo.Death();
        GameObject effect = ObjectPool.instance.Create(charmEffect);
        effect.transform.SetParent(target.transform);
        effect.transform.localPosition = Vector3.zero;
        target.animatorController.ChangeColor(color);
        target.StartCoroutine(Wait());
    }
    IEnumerator Wait()
    {
        yield return null;
        target.stateController.ChangeState(StateName.IdleState);
    }
    public override void BuffOver()
    {
        base.BuffOver();
    }
}


/// <summary>
/// 愤怒 Buff：TimeBuff + 攻速增益 + 减防（额外承伤）+ 特效
/// </summary>
public class AngryBuff : TimeBuff
{
    [LabelText("生气特效")]
    public GameObject angryEffect;
    [SerializeReference] public Buff_BaseValueBuff_AttackSpeed attackSpeedBuff;
    [SerializeReference] public Buff_BaseValueBuff_ExtraDefence extraDefenceBuff;
    GameObject effect;
    void EnsureBuffs()
    {
        if (attackSpeedBuff == null) { attackSpeedBuff = new Buff_BaseValueBuff_AttackSpeed { speed = 0.5f }; }
        if (extraDefenceBuff == null) { extraDefenceBuff = new Buff_BaseValueBuff_ExtraDefence { extraDefence = -1f }; }
    }
    protected override void PrepareForRestore() => EnsureBuffs();
    public override void BuffEffect(Chess target)
    {
        EnsureBuffs();
        base.BuffEffect(target);
        attackSpeedBuff.target = target; attackSpeedBuff.BuffEffect(target);
        extraDefenceBuff.target = target; extraDefenceBuff.BuffEffect(target);
        if (angryEffect != null) { effect = ObjectPool.instance.Create(angryEffect); effect.transform.SetParent(target.transform); effect.transform.localPosition = Vector3.zero; }
    }
    public override void BuffOver()
    {
        if (extraDefenceBuff != null) extraDefenceBuff.BuffOver();
        if (attackSpeedBuff != null) attackSpeedBuff.BuffOver();
        if (effect != null) { ObjectPool.instance.Recycle(effect); effect = null; }
        base.BuffOver();
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        var other = resetBuff as AngryBuff;
        if (other?.attackSpeedBuff != null && attackSpeedBuff != null) attackSpeedBuff.BuffReset(other.attackSpeedBuff);
        if (other?.extraDefenceBuff != null && extraDefenceBuff != null) extraDefenceBuff.BuffReset(other.extraDefenceBuff);
    }
}

/// <summary>
/// 缴械buff
/// </summary>
public class DisarmBuff : Buff
{
    public GameObject disaarnEffect;//缴械特效
    public DisarmBuff()
    {
        buffName = "缴械";
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.equipWeapon.AttackAble = false;
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.equipWeapon.AttackAble = true;
    }

}
#endregion
#region 治疗类buff
/// <summary>
/// 恢复buff
/// </summary>
public class ResumeBuff : Buff
{
    protected Timer timer;
    public float healPercent = 0.05f;
    public float healRate = 1;
    public override void WriteToSaveData(BuffSaveData data)
    {
        base.WriteToSaveData(data);
        if (data != null && timer != null) data.remainingTime = timer.LeftTime();
    }
    public override void RestoreFromSaveData(BuffSaveData data)
    {
        base.RestoreFromSaveData(data);
        if (data != null && data.remainingTime >= 0) _restoreRemainingTime = data.remainingTime;
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        float value = 0.5f + target.propertyController.GetHpPerCent();
        target.animatorController.ChangeColor(new Color(1, 1, 1, value));
        float delay = _restoreRemainingTime >= 0 ? _restoreRemainingTime : healRate;
        _restoreRemainingTime = -1f;
        timer = GameManage.instance.timerManage.AddTimer(Heal, delay, true);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        timer.Stop();
        timer = null;
        target.animatorController.ChangeColor(Color.white);
    }
    public void Heal()
    {
        target.propertyController.Heal(target.propertyController.GetMaxHp() * healPercent);
        float value = 0.5f + target.propertyController.GetHpPerCent();
        target.animatorController.ChangeColor(new Color(1, 1, 1, value));
        if (target.propertyController.GetHpPerCent() >= 0.999)
        {
            BuffOver();
        }
    }
}
#endregion
#region 场地Buff
/// <summary>
/// 上课 Buff：TimeBuff + 减伤 + 减移速 + 与下课互斥
/// </summary>
public class Buff_ClassBegin : TimeBuff
{
    [SerializeReference] public Buff_BaseValueBuff_ExtraDefence extraDefenceBuff;
    [SerializeReference] public Buff_BaseValueBuff_AcceleRate acceleRateBuff;
    [UnityEngine.Serialization.FormerlySerializedAs("extradefence")] public float _extraDefence = 0.3f;
    [UnityEngine.Serialization.FormerlySerializedAs("extraSpeed")] public float _extraSpeed = 0.25f;
    void EnsureBuffs()
    {
        if (extraDefenceBuff == null) extraDefenceBuff = new Buff_BaseValueBuff_ExtraDefence { extraDefence = _extraDefence };
        if (acceleRateBuff == null) acceleRateBuff = new Buff_BaseValueBuff_AcceleRate { rate = -_extraSpeed };
    }
    protected override void PrepareForRestore() => EnsureBuffs();
    public override void BuffEffect(Chess target)
    {
        EnsureBuffs();
        base.BuffEffect(target);
        extraDefenceBuff.target = target; extraDefenceBuff.BuffEffect(target);
        acceleRateBuff.target = target; acceleRateBuff.BuffEffect(target);
        if (target.buffController.buffDic.TryGetValue("下课", out var buff)) buff.BuffOver();
    }
    public override void BuffOver()
    {
        if (extraDefenceBuff != null) extraDefenceBuff.BuffOver();
        if (acceleRateBuff != null) acceleRateBuff.BuffOver();
        base.BuffOver();
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        var other = resetBuff as Buff_ClassBegin;
        if (other?.extraDefenceBuff != null && extraDefenceBuff != null) extraDefenceBuff.BuffReset(other.extraDefenceBuff);
        if (other?.acceleRateBuff != null && acceleRateBuff != null) acceleRateBuff.BuffReset(other.acceleRateBuff);
    }
}
/// <summary>
/// 下课 Buff：TimeBuff + 加攻 + 加移速 + 与上课互斥
/// </summary>
public class Buff_ClassOver : TimeBuff
{
    [SerializeReference] public Buff_BaseValueBuff_Attack attackBuff;
    [SerializeReference] public Buff_BaseValueBuff_AcceleRate acceleRateBuff;
    [UnityEngine.Serialization.FormerlySerializedAs("extraAttack")] public float _extraAttack = 0.3f;
    [UnityEngine.Serialization.FormerlySerializedAs("extraSpeed")] public float _extraSpeed = 0.25f;
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
        attackBuff.target = target; attackBuff.BuffEffect(target);
        acceleRateBuff.target = target; acceleRateBuff.BuffEffect(target);
        if (target.buffController.buffDic.TryGetValue("上课", out var buff)) buff.BuffOver();
    }
    public override void BuffOver()
    {
        if (attackBuff != null) attackBuff.BuffOver();
        if (acceleRateBuff != null) acceleRateBuff.BuffOver();
        base.BuffOver();
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        var other = resetBuff as Buff_ClassOver;
        if (other?.attackBuff != null && attackBuff != null) attackBuff.BuffReset(other.attackBuff);
        if (other?.acceleRateBuff != null && acceleRateBuff != null) acceleRateBuff.BuffReset(other.acceleRateBuff);
    }
}

#endregion


/// <summary>
/// 这个buff的作用就是创建一个衍生物
/// </summary>
public class Buff_Create:Buff
{
    public PropertyCreator creator;
    Chess user;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        user = target;
        PrePlantImage_Data data = new PrePlantImage_Data();
        data.creator = creator;
        data.preSprite = creator.chessSprite;
        data.tag=target.tag;
        PrePlantImage.instance.TryToPlant(CancelPlant,PlantOver,data,HandItemType.Plants);
    }

    public void CancelPlant()
    {
        user.skillController.activeSkill.ReturnCD();
        BuffOver();
    }
    public void PlantOver(Chess target)
    {
        //Debug.Log("创建了 "+target); 为什么第二次用这个技能的时候会秒种呢 而且还种下去了 hyw呢
        //debug
        user.skillController.context.Set<Chess>(buffName, target);
        target.OnRemove.AddListener(OnPlantDeath);
        BuffOver();
    }
    public void OnPlantDeath(Chess target)
    {
        //Debug.Log("移除了" + target.name);
        user.skillController.context.Remove(buffName);
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        Debug.LogError("不对，你怎么能同时拥有两个这个buff");
    }
    public override void BuffOver()
    {
        base.BuffOver();

    }
}
