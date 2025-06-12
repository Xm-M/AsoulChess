using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// 我感觉我这个buff系统写的也很垃圾 
/// </summary>
[Serializable]
public abstract class Buff
{
    [LabelText("Buff名")]
    public string buffName;//这个buff的名字
    [HideInInspector] public Chess target;//buff的作用对象
    [HideInInspector] public int buffNum;//buff的叠加层数
    public virtual void BuffReset()
    {
        buffNum++;
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

    }
    public override void BuffReset()
    {
        timer.ResetTime();
    }
    public override void BuffOver()
    {
        base.BuffOver();
        timer?.Stop();
        timer = null;
    }
}
/// <summary>
/// 
/// </summary>
public class CrazyBuff : Buff
{
    [LabelText("buff持续时间")]
    public float continueTime=10f;
    protected Timer timer;
    [LabelText("额外攻速")]
    public float addspeed=0.5f;
    
    public override void BuffEffect(Chess target)
    {

        base.BuffEffect(target);
        //Debug.Log("加攻速");
        AddSpeed(target);
        timer=GameManage.instance.timerManage.AddTimer(BuffOver,continueTime,false);
    }
    public override void BuffReset()
    {
        timer.ResetTime();

    }
    public override void BuffOver()
    {
        base.BuffOver();
        timer?.Stop();
        timer = null;
        RemoveSpeed(target);
        //Debug.Log("加速结束");
    }
    public void AddSpeed(Chess target)
    {
        target.propertyController.ChangeAcceleRate(addspeed);
        
    }
    public void RemoveSpeed(Chess target)
    {
        target.propertyController.ChangeAcceleRate(-addspeed);
    }
}
/// <summary>
/// 喝了点b酒下手没轻没重的 
/// 拥有该buff的单位
/// </summary>
public class Buff_Wine : TimeBuff
{
    [LabelText("额外暴击率")]
    public float extraCrite=0.25f;
    [LabelText("减速效率")]
    public float slowRate=-0.25f;
    public override void BuffEffect(Chess target)
    {
        Debug.Log("喝酒了");
        base.BuffEffect(target);
        target.propertyController.ChangeAcceleRate(slowRate);
        target.propertyController.ChangeCrit(extraCrite);
        timer = GameManage.instance.timerManage.AddTimer(BuffOver, continueTime, false);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeAcceleRate(-slowRate);
        target.propertyController.ChangeCrit(-extraCrite);
    }
}
public class ColdBuff : TimeBuff
{
    [LabelText("减速效率")]
    public float slowRate = -0.5f;
    public GameObject coldBuff;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        if (this.coldBuff != null)
        {
            GameObject cold = ObjectPool.instance.Create(coldBuff);
            cold.transform.position = target.transform.position;
        }
        target.propertyController.ChangeAcceleRate(slowRate);
        target.animatorController.ChangeColor(Color.blue);
        timer = GameManage.instance.timerManage.AddTimer(BuffOver, continueTime, false);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeAcceleRate(-slowRate);
        target.animatorController.ChangeColor(Color.white);
    }
}


public class DizznessBuff : TimeBuff
{
    StateName current;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        //Debug.Log("剩余时间" + continueTime);
        timer = GameManage.instance.timerManage.AddTimer(BuffOver, continueTime, false);
        current = target.stateController.currentState.state.stateName;
        target.animatorController.ChangeSpeed(0);
        target.stateController.ChangeState(StateName.DizzyState);

    }
    public override void BuffOver()
    {
        base.BuffOver();
        //Debug.Log("重力场结束");
        target.stateController.ChangeState(current);
    }
}
public class FreezyBuff : DizznessBuff
{
    [SerializeReference]
    public ColdBuff buff;//这里的coldbuff 不要加音效 但是名字是一样的
    public GameObject FreezyEffect; //这个也是没有音效的
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        if (this.FreezyEffect != null)
        {
            GameObject cold = ObjectPool.instance.Create(FreezyEffect);
            cold.transform.position = target.transform.position;
        }
        target.buffController.AddBuff(buff);
    }
    public override void BuffOver()
    {
        base.BuffOver();

    }
}



/// <summary>
/// 恢复buff
/// </summary>
public class ResumeBuff : Buff
{
    Timer timer;
    public float healPercent=0.05f;
    public float healRate=1;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        float value = 0.5f + target.propertyController.GetHpPerCent();
        target.animatorController.ChangeColor(new Color(1, 1, 1, value));
        timer = GameManage.instance.timerManage.AddTimer(Heal, healRate, true);
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
        target.propertyController.Heal(target.propertyController.GetMaxHp()*healPercent);
        float value = 0.5f + target.propertyController.GetHpPerCent();
        target.animatorController.ChangeColor(new Color(1, 1, 1, value));
        if (target.propertyController.GetHpPerCent() >= 0.999)
        {
            BuffOver();
        }
    }

    public override void BuffReset()
    {
        base.BuffReset();
    }
    
}
/// <summary>
/// 抹茶芭菲buff
/// </summary>
public class MatchaParfaitBuff : TimeBuff
{
    [SerializeReference]
    public ColdBuff coldBuff;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.onTakeDamage.AddListener(OnTakeDamage);
        timer=GameManage.instance.timerManage.AddTimer(BuffOver,continueTime, false);
        //效果是如果目标是要乐奈 则提供效果(转换状态什么的)
        if (target.propertyController.creator.name == "要乐奈")
        {
            target.animatorController.animator.SetBool("Match", true);
        }
    }
    public override void BuffReset()
    {
        base.BuffReset();
        if (target.propertyController.creator.name == "要乐奈")
        {
            target.animatorController.animator.SetBool("Match", true);
        }
    }
    public void OnTakeDamage(DamageMessege dm)
    {
        dm.damageTo.buffController.AddBuff(coldBuff);
    }
    
    public override void BuffOver()
    {
        target.propertyController.onTakeDamage.RemoveListener(OnTakeDamage);
        base.BuffOver();
        
    }
}
public class BloodBuff : TimeBuff
{
    public DamageMessege dm;
    public float damage = 70;
    float speed;
    public override void BuffEffect(Chess target)
    {
        dm.damageTo = target;
        dm.damageFrom = target;
        this.target = target;
        speed = UnityEngine.Random.Range(1, 2f);
        timer = GameManage.instance.timerManage.AddTimer(BloodDamage,speed*Time.deltaTime,true);
        //base.BuffEffect(target);
    }
    public void BloodDamage()
    {
        dm.damage = damage* Time.deltaTime;
        target.propertyController.GetDamage(dm);
        
    }
    public override void BuffOver()
    {
        //Debug.Log("流血buff结束");
        timer.Stop();
        timer = null;
        base.BuffOver();
       
    }
}
public class BloodDeBuff : TimeBuff
{
    public DamageMessege dm;
    public float damage = 70;
    Timer damagetimer;
    public override void BuffEffect(Chess target)
    {
        dm.damageTo = target;
        dm.damageFrom = target;
        this.target = target;
        //speed = UnityEngine.Random.Range(1, 2f);
        damagetimer = GameManage.instance.timerManage.AddTimer(BloodDamage, 1, true);
        timer = GameManage.instance.timerManage.AddTimer(BuffOver, continueTime);
        //base.BuffEffect(target);
    
    }
     
    public void BloodDamage()
    {
        dm.damage = damage;
        target.propertyController.GetDamage(dm);

    }
    public override void BuffOver()
    {
        //Debug.Log("流血buff结束");
        timer.Stop();
        damagetimer.Stop();
        damagetimer = null;
        timer = null;
        base.BuffOver();
        
    }
}

/// <summary>
/// 偷摸零那使用的 削弱抗性的buff 好吧 估计不是偷摸零用了
/// </summary>
public class Buff_Weak :TimeBuff
{
    public float weakArmor;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.ChangeAR(-weakArmor);
        timer = GameManage.instance.timerManage.AddTimer(BuffOver, continueTime);
    }
    public override void BuffReset()
    {
        base.BuffReset();
        timer.ResetTime();
    }
    public override void BuffOver()
    {
        target.propertyController.ChangeAR(weakArmor);
        base.BuffOver();
    }
}

/// <summary>
/// 威压buff tomo，saki，taki的攻击都会施加这个buff
/// 那么威压到底会导致什么呢 输出降低？抗性降低？那就降低攻击力和防御力
/// </summary>
public class Buff_Coercion : TimeBuff
{
    [LabelText("减攻")]
    public float deAttack=0.2f;//减少的攻击力
    [LabelText("减防")]
    public float deDefence = 0.2f;//减少的防御力
    [LabelText("虚弱特效")]
    public GameObject Coerctioneffect;
    //GameObject effect;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        timer = GameManage.instance.timerManage.AddTimer(BuffOver, continueTime);
        target.propertyController.ChangeAttack(-deAttack);
        target.propertyController.ChangeExtraDefence(-deDefence);
        GameObject effect =ObjectPool.instance.Create(Coerctioneffect);
        effect.transform.SetParent(target.transform);
        effect.transform.localPosition = Vector3.zero;
    }
    public override void BuffReset()
    {
        base.BuffReset();
        GameObject effect = ObjectPool.instance.Create(Coerctioneffect);
        effect.transform.SetParent(target.transform);
        effect.transform.localPosition = Vector3.zero;
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeAttack(-deAttack);
        target.propertyController.ChangeExtraDefence(-deDefence);
        //ObjectPool.instance.ReycleObject(effect);
    }
}

/// <summary>
/// 现在重点是恐惧的回头走两步要怎么实现...
/// </summary>
public class Buff_Fear : TimeBuff {

    [LabelText("恐惧特效")]
    public GameObject FearEffect;
    [LabelText("缓速效率")]
    public float moveRate=0.5f;
    GameObject effect;
    StateName current;
    Tile stand;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        current = target.stateController.currentState.state.stateName;
        timer = GameManage.instance.timerManage.AddTimer(BuffOver, continueTime, false);
        effect = ObjectPool.instance.Create(FearEffect);
        effect.transform.SetParent(target.transform);
        effect.transform.localPosition = Vector3.zero;
        target.stateController.ChangeState(StateName.DizzyState);
        stand = target.moveController.nextTile;
        if (target.moveController.standTile != null) {
            float speed = target.propertyController.GetMoveSpeed()*moveRate;
            target.moveController.MoveToTarget(target.moveController.standTile,speed
                 );
            target.animatorController.PlayMove();
            target.transform.right=-target.transform.right;
        }
    }
    
    public override void BuffOver()
    {
        base.BuffOver();
        //effect.transform.SetParent(null);
        ObjectPool.instance.Recycle(effect);
        target.stateController.ChangeState(current);
        target.moveController.StopMove();
        target.transform.right = -target.transform.right;
        target.moveController.nextTile = stand;
    }
}
public class Buff_Mygo : Buff
{
    [LabelText("额外增伤")]
    public float extraDamage=0.2f;
    [LabelText("额外减伤")]
    public float extraDefence=0.2f;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.ChangeExtraDamage(extraDamage);
        target.propertyController.ChangeExtraDefence(extraDefence);
    
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeExtraDamage(-extraDamage);
        target.propertyController.ChangeExtraDefence(-extraDefence);
        
    }
}
/// <summary>
/// 魅惑buff
/// </summary>
public class Buff_Charm : Buff
{
    public Color color;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        ChessTeamManage.Instance.ChangeTeam(target);
        target.transform.right = -target.transform.right;
        //dm.damageTo.Death();
        target.animatorController.ChangeColor(color);
        target.stateController.ChangeState(StateName.IdleState);
    }
    public override void BuffOver()
    {
        base.BuffOver();
    }
}