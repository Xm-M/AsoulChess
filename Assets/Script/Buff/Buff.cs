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
    //StateName current;
    public GameObject Dizznesseffect;
    GameObject effect;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        //current = target.stateController.currentState.state.stateName;
        target.propertyController.ChangeDizznessTime(continueTime);
        timer = GameManage.instance.timerManage.AddTimer(BuffOver, target.propertyController.GetDizznessTime(), false);
        if (Dizznesseffect != null)
        {
            effect = ObjectPool.instance.Create(Dizznesseffect);
            effect.transform.SetParent(target.transform);
            effect.transform.localPosition = Vector3.zero;
        }

    }
    public override void BuffReset()
    {
        base.BuffReset();
        //Debug.Log("重置buff");
        target.propertyController.ChangeDizznessTime(continueTime);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        if (effect != null)
        {
            ObjectPool.instance.Recycle(effect);
            effect = null;
        }
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
        target.animatorController.Freezy();
        target.buffController.AddBuff(buff);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.animatorController.ResumeSpeed();
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
        speed = UnityEngine.Random.Range(1, 1.5f);
        timer = GameManage.instance.timerManage.AddTimer(BloodDamage,speed*Time.deltaTime,true);
        //base.BuffEffect(target);
    }
    public void BloodDamage()
    {
        dm.damage = damage* Time.deltaTime;
        if(!target.IfDeath)
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
        
        effect = ObjectPool.instance.Create(FearEffect);
        effect.transform.SetParent(target.transform);
        effect.transform.localPosition = Vector3.zero;
        //target.stateController.ChangeState(StateName.DizzyState);
        target.propertyController.ChangeDizznessTime(continueTime);
        timer = GameManage.instance.timerManage.AddTimer(BuffOver, target.propertyController.GetDizznessTime(), false);
        stand = target.moveController.nextTile;
        if (target.moveController.standTile != null&&target.propertyController.GetMoveSpeed()!=0) {
            float speed = target.propertyController.GetMoveSpeed()*moveRate;
            target.moveController.MoveToTarget(target.moveController.standTile,speed
                 );
            target.animatorController.PlayMove();
            target.transform.right=-target.transform.right;
        }
        else
        {
            target.animatorController.PlayIdle();
        }
    }
    public override void BuffReset()
    {
        base.BuffReset();
        target.propertyController.ChangeDizznessTime(continueTime);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        //effect.transform.SetParent(null);
        ObjectPool.instance.Recycle(effect);
        target.stateController.ChangeState(current);
        if (target.moveController.standTile != null && target.propertyController.GetMoveSpeed() !=0) {
            target.moveController.StopMove();
            target.transform.right = -target.transform.right;
            target.moveController.nextTile = stand; 
        }
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
/// 吉野家的牛肉饭 吃完后加20%攻击力 如果目标是小孩姐 则切换成
/// </summary>
public class BeafRiceBuff : Buff
{
    [LabelText("额外百分比攻击力")]
    public float extraAttack=0.2f;
    [LabelText("修改攻击距离")]
    public float closeAttackRange = 3.75f;
    [LabelText("额外攻速")]
    public float extraSpeed=0.25f;
    float baseAttackRange;
    public GameObject effect;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);

        target.propertyController.ChangeAttack(extraAttack);        
        if (target.propertyController.creator.chessName == "井芹仁菜")
        {
            //target.animatorController.ChangeFloat("")
            if (target.animatorController.animator.GetFloat("type") < 1)
            {
                baseAttackRange = target.propertyController.GetAttackRange();
                target.animatorController.ChangeFloat("type", 1);
                target.animatorController.animator.SetInteger("skill1", 1);
                Debug.Log("第一次吃");
                target.propertyController .SetAtttackRange(closeAttackRange);
                target.propertyController.ChangeAttack(extraAttack*4f);
                //target.propertyController.ChangeAcceleRate(extraSpeed);
            }
            else 
            {
                target.animatorController.animator.SetInteger("skill1", 2);
                target.animatorController.ChangeFloat("type", 2);
                Debug.Log("第二次吃");
            }
        }
        if (effect != null)
        {
            GameObject newEffect= ObjectPool.instance.Create(effect);
            newEffect.transform.position = target.transform.position;
        }
    }
    public override void BuffReset()
    {
        base.BuffReset();
        if (effect != null)
        {
            GameObject newEffect = ObjectPool.instance.Create(effect);
            newEffect.transform.position = target.transform.position;
        }
        if (target.propertyController.creator.chessName == "井芹仁菜")
        {
            //target.animatorController.ChangeFloat("")
            if (target.animatorController.animator.GetFloat("type") < 1)
            {
                baseAttackRange = target.propertyController.GetAttackRange();
                target.animatorController.ChangeFloat("type", 1);
                target.animatorController.animator.SetInteger("skill1", 1);
                Debug.Log("第一次吃");
                target.propertyController.SetAtttackRange(closeAttackRange);
            }
            else
            {
                target.animatorController.animator.SetInteger("skill1", 2);
                target.animatorController.ChangeFloat("type", 2);
                Debug.Log("第二次吃");
            }
        }
    }
     
    public override void BuffOver()
    {
        base.BuffOver();
        target.animatorController.ChangeFloat("type", 0);
        target.propertyController.ChangeAttack(-extraAttack);
        if (target.propertyController.creator.name == "井芹仁菜")
        {
            target.animatorController.ChangeFloat("type", 0);
            target.propertyController.SetAtttackRange(baseAttackRange);
            target.propertyController.ChangeAttack(-extraAttack * 4f);
            //target.propertyController.ChangeAcceleRate(-extraSpeed);
        }
    }
}
public class DodgeBuff : TimeBuff
{
    [LabelText("额外闪避率")]
    public float extraDodgeRate = 1f;
    public GameObject effect;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.ChangeDodgeRate(extraDodgeRate);
        timer = GameManage.instance.timerManage.AddTimer(BuffOver, continueTime);
        if (effect != null)
        {
            GameObject newEffect = ObjectPool.instance.Create(effect);
            newEffect.transform.position = target.transform.position;
        }
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeDodgeRate(-extraDodgeRate);
    }
    public override void BuffReset()
    {
        base.BuffReset();
        timer.ResetTime();
        if (effect != null)
        {
            GameObject newEffect = ObjectPool.instance.Create(effect);
            newEffect.transform.position = target.transform.position;
        }
    }
}
/// <summary>
/// 愤怒Buff
/// </summary>
public class AngryBuff:Buff
{
    [LabelText("生气特效")]
    public GameObject angryEffect;
    [LabelText("额外攻速")]
    public float extraAttackSpeed=0.5f;
    [LabelText("额外承伤")]
    public float extraTake=1f;
    GameObject effect;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.ChangeExtraDefence(-extraTake);
        target.propertyController.ChangeAcceleRate(extraAttackSpeed);
        effect = ObjectPool.instance.Create(angryEffect);
        effect.transform.SetParent(target.transform);
        effect.transform.localPosition = Vector3.zero;
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeExtraDefence(extraTake);
        target.propertyController.ChangeAcceleRate(-extraAttackSpeed);
        ObjectPool.instance.Recycle(effect);
    }
    public override void BuffReset()
    {
        base.BuffReset();
        
    }
}
public class Buff_MMKFirm : Buff
{
    [LabelText("冷却时间")]
    public float coldDonw = 15f;
    [LabelText("倍率基础")]
    public float buffBase=40;
    public DamageMessege Dm;
     
    public GameObject user;
    int buffCount;
    Timer timer;
    Timer triggerTimer;
    bool isCold;
    bool isTrigger;
    int count;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        buffCount++;
        isCold = true;
        if (target.CompareTag(user.tag))
        {
            
            target.propertyController.onSetDamage.AddListener(OnGetDamage);
            timer = GameManage.instance.timerManage.AddTimer(() => isCold = true, coldDonw, true);
        }
    }
    public override void BuffOver()
    {
        base.BuffOver();
        buffCount = 0;
        if (target.CompareTag(user.tag))
        {
            target.propertyController.onSetDamage.RemoveListener(OnGetDamage);
            timer?.Stop();
            triggerTimer?.Stop();
            timer = null;
        }
    }
    public override void BuffReset()
    {
        base.BuffReset();
        buffCount++;
    }
    public void TriggerBuffEffect()
    {
        //Debug.Log("MMK触发效果");
        if (target.CompareTag(user.tag))
        {
            target.propertyController.Heal(buffCount * buffBase);
        }
        else
        {
            Dm.damageFrom = null;
            Dm.damage = buffCount * buffBase;
            Dm.damageTo = target;
            target.propertyController.GetDamage(Dm);
            
        }
        
    }
    public void OnGetDamage(DamageMessege dm)
    {
        if ((dm.damage >= dm.damageTo.propertyController.GetHp()&&isCold)||isTrigger)
        {
            dm.damage = 0;
            Debug.Log("触发不屈");
            UIManage.GetView<DamagePanel>().ShowText(dm, "不屈!", Color.white);
            if (!isTrigger)
            {
                isTrigger=true;
                count=buffCount;
                //dm.damageTo.StartCoroutine(Firm(dm.damageTo));
                triggerTimer = GameManage.instance.timerManage.AddTimer(Firm, 1, true);
            }
        }
    }
    public void Firm()
    {
        if (count > 0)
        {
            count--;
        }
        else
        {
            isTrigger = false;
            isCold = false;
            timer.ResetTime();
            triggerTimer.Stop();
            triggerTimer = null;
        }
    }
}
/// <summary>
/// 持续10s 上课buff
/// </summary>
public class Buff_ClassBegin : TimeBuff
{
    [LabelText("额外减伤")]
    public float extradefence=0.3f;
    [LabelText("减少移速")]
    public float extraSpeed=0.25f;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.ChangeExtraDefence(extraSpeed);
        target.propertyController.ChangeAcceleRate(-extraSpeed);
        Buff buff = null;
        target.buffController.buffDic.TryGetValue("下课",out buff);
        if(buff != null)
        {
            buff.BuffOver();
        }
        timer = GameManage.instance.timerManage.AddTimer(BuffOver, continueTime);
    }
    public override void BuffReset()
    {
        base.BuffReset();
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeExtraDefence(-extraSpeed);
        target.propertyController.ChangeAcceleRate(extraSpeed);
    }
}
/// <summary>
/// 下课buff 持续50s
/// </summary>
public class Buff_ClassOver : TimeBuff
{
    [LabelText("额外攻击")]
    public float extraAttack=0.3f;
    [LabelText("额外移速")]
    public float extraSpeed=0.25f;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.ChangeAttack(extraAttack);
        target.propertyController.ChangeAcceleRate(extraSpeed);
        Buff buff = null;
        target.buffController.buffDic.TryGetValue("上课", out buff);
        if (buff != null)
        {
            buff.BuffOver();
        }
        timer = GameManage.instance.timerManage.AddTimer(BuffOver, continueTime);
    }
    public override void BuffOver()
    {
        base.BuffOver();
    }
    public override void BuffReset()
    {
        base.BuffReset();
        target.propertyController.ChangeAttack(-extraAttack);
        target.propertyController.ChangeAcceleRate(-extraSpeed);
    }
}
