using System;
using UnityEngine;

/// <summary>
/// Doloris 协战：携带者在 <see cref="PropertyController.onTakeDamage"/> 时，由 <see cref="buffFrom"/> 对本次命中的敌人追加魔法伤害。
/// </summary>
[Serializable]
public class Buff_Mujica_Doloris : Buff
{
    public Chess buffFrom;
    public float damageRate = 0.3f;

    public Buff_Mujica_Doloris()
    {
        buffName = "Mujica_Doloris";
    }

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        // 读档无法还原 runtime Chess 引用时跳过，避免 NRE
        if (buffFrom == null || buffFrom.propertyController == null ||
            target == null || target.propertyController == null)
        {
            BuffOver();
            return;
        }
        if (buffFrom.propertyController.creator == target.propertyController.creator)
        {
            BuffOver();
            return;
        } 
        target.propertyController.onTakeDamage.AddListener(OnTakeDamage);
    }

    public override void BuffOver()
    {
        if (target != null && target.propertyController != null&&target!=buffFrom)
            target.propertyController.onTakeDamage.RemoveListener(OnTakeDamage);
        base.BuffOver();
    }

    void OnTakeDamage(DamageMessege dm)
    {
        if (buffFrom == null || buffFrom.IfDeath || target == null || target.IfDeath)
            return;
        if (dm == null || dm.damageType == DamageType.Heal || dm.damageTo == null || dm.damageTo.IfDeath)
            return;
        if (dm.damageFrom != target)
            return;

        float atk = buffFrom.propertyController.GetAttack();
        float dmg = damageRate * atk;
        if (dmg <= 0f)
            return;

        var mes = new DamageMessege(buffFrom, dm.damageTo, dmg, DamageType.Magic, ElementType.Cutting);
        buffFrom.propertyController.TakeDamage(mes);
    }

    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        if (resetBuff is Buff_Mujica_Doloris o && o.damageRate > damageRate)
        {
            damageRate = o.damageRate;
            buffFrom = o.buffFrom;
        }
    }
}

/// <summary>
/// 诗人持续治疗：每 1 秒治疗一次，治疗量 = <see cref="buffFrom"/> 当前攻击力 × <see cref="healRate"/>。
/// </summary>
[Serializable]
public class Buff_Bard : Buff
{
    //public Chess buffFrom;
    [Tooltip("治疗系数，默认 0.1 = 10% 攻击力")]
    public float healRate  ;
    [Tooltip("Buff 持续时间（秒），被动每秒刷新时会 BuffReset 续期")]
    public float continueTime = 6f;

    Timer _healRepeat;
    Timer _duration;

    public Buff_Bard()
    {
        buffName = "Bard_Doloris";
    }

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        if (GameManage.instance == null || GameManage.instance.timerManage == null)
            return;
        var tm = GameManage.instance.timerManage;
        _healRepeat = tm.AddTimer(OnHealTick, 1f, true);
        _duration = tm.AddTimer(BuffOver, continueTime, false);
    }

    void OnHealTick()
    {
        float h =  healRate;
        if (h > 0f)
            target.propertyController.Heal(h);
    }

    public override void BuffOver()
    {
        _healRepeat?.Stop();
        _duration?.Stop();
        _healRepeat = null;
        _duration = null;
        base.BuffOver();
    }

    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        Buff_Bard o = resetBuff as Buff_Bard;
        if (o == null) return;
        if (o.healRate > healRate)
            healRate = o.healRate;
 
        continueTime = o.continueTime;
        _duration?.ResetTime(continueTime);
    }
}
