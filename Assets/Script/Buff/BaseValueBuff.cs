using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这属于纯增益buff 所以同命名buff取最高值
/// </summary>
public class Buff_BaseValueBuff : Buff
{
    public GameObject effect;
    /// <summary>供存读档使用，返回主数值（float）</summary>
    public virtual float GetSaveValue() => 0;
    /// <summary>供存读档使用，设置主数值</summary>
    public virtual void SetSaveValue(float v) { }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        if (effect != null)
        {
            GameObject newEffect = ObjectPool.instance.Create(effect);
            newEffect.transform.position = target.transform.position;
        }
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        if (effect != null)
        {
            GameObject newEffect = ObjectPool.instance.Create(effect);
            newEffect.transform.position = target.transform.position;
        }
    }
}

public class Buff_BaseValueBuff_Attack: Buff_BaseValueBuff
{
    public float extraAttack;
    public override float GetSaveValue() => extraAttack;
    public override void SetSaveValue(float v) => extraAttack = v;
    public Buff_BaseValueBuff_Attack()
    {
        buffName = "攻击增益";
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.ChangeAttack(extraAttack);
        Debug.Log("增加了攻击力" + extraAttack);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeAttack(-extraAttack);
        Debug.Log("减少了攻击力" + -extraAttack);
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        //target.propertyController.ChangeAttack(-extraAttack);
        float ex = (resetBuff as Buff_BaseValueBuff_Attack).extraAttack;
        //同类型buff取最高值
        if (ex > extraAttack)
        {
            target.propertyController.ChangeAttack(-extraAttack);
            extraAttack = ex;
            target.propertyController.ChangeAttack(extraAttack);
        }
    }
}
public class Buff_BaseValueBuff_AttackSpeed : Buff_BaseValueBuff
{
    public float speed;
    public override float GetSaveValue() => speed;
    public override void SetSaveValue(float v) => speed = v;
    public Buff_BaseValueBuff_AttackSpeed()
    {
        buffName = "攻速增益";
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.ChangeAcceleRate(speed);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeAcceleRate(-speed);
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        
        float sp = (resetBuff as Buff_BaseValueBuff_AttackSpeed).speed;
        if (sp > speed)
        {
            target.propertyController.ChangeAcceleRate(-speed);
            speed = sp; 
            target.propertyController.ChangeAcceleRate(+speed);
        }
    }
}
public class Buff_BaseValueBuff_HPmax : Buff_BaseValueBuff
{
    public float hpmax;
    public override float GetSaveValue() => hpmax;
    public override void SetSaveValue(float v) => hpmax = v;
    public Buff_BaseValueBuff_HPmax()
    {
        buffName = "生命值增益";
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.ChangeHPMax(hpmax);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeHPMax(-hpmax);
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        float hm = (resetBuff as Buff_BaseValueBuff_HPmax).hpmax;
        if (hm>hpmax) {
            target.propertyController.ChangeHPMax(hpmax);
            hpmax = hm;
            target.propertyController.ChangeHPMax(-hpmax);
        }
    }

}
public class Buff_BaseValueBuff_Crit : Buff_BaseValueBuff
{
    public float crit;
    public override float GetSaveValue() => crit;
    public override void SetSaveValue(float v) => crit = v;
    public Buff_BaseValueBuff_Crit()
    {
        buffName = "暴击增益";
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.ChangeCrit(crit);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeCrit(-crit);
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        float cr = (resetBuff as Buff_BaseValueBuff_Crit).crit;
        if (cr >crit) {
            target.propertyController.ChangeCrit(crit);
            crit = cr;
            target.propertyController.ChangeCrit(-crit);
        }
    }
}
public class Buff_BaseValueBuff_DodgeRate : Buff_BaseValueBuff
{
    [LabelText("额外闪避率")]
    public float extraDodgeRate = 1f;
    public override float GetSaveValue() => extraDodgeRate;
    public override void SetSaveValue(float v) => extraDodgeRate = v;
    public  Buff_BaseValueBuff_DodgeRate()
    {
        buffName = "闪避增益";
    }
    
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.ChangeDodgeRate(extraDodgeRate);

    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeDodgeRate(-extraDodgeRate);
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        float dg=(resetBuff as Buff_BaseValueBuff_DodgeRate).extraDodgeRate;
        if (dg > extraDodgeRate)
        {
            target.propertyController.ChangeDodgeRate(-extraDodgeRate);
            extraDodgeRate=dg;
            target.propertyController.ChangeDodgeRate(extraDodgeRate);
        }
       
    }
}

/// <summary>额外减伤/承伤</summary>
public class Buff_BaseValueBuff_ExtraDefence : Buff_BaseValueBuff
{
    public float extraDefence;
    public override float GetSaveValue() => extraDefence;
    public override void SetSaveValue(float v) => extraDefence = v;
    public Buff_BaseValueBuff_ExtraDefence() { buffName = "减伤增益"; }
    public override void BuffEffect(Chess target) { base.BuffEffect(target); target.propertyController.ChangeExtraDefence(extraDefence); }
    public override void BuffOver() { base.BuffOver(); target.propertyController.ChangeExtraDefence(-extraDefence); }
    public override void BuffReset(Buff resetBuff) { base.BuffReset(resetBuff); float v = (resetBuff as Buff_BaseValueBuff_ExtraDefence).extraDefence; if (v > extraDefence) { target.propertyController.ChangeExtraDefence(-extraDefence); extraDefence = v; target.propertyController.ChangeExtraDefence(extraDefence); } }
}

/// <summary>额外增伤</summary>
public class Buff_BaseValueBuff_ExtraDamage : Buff_BaseValueBuff
{
    public float extraDamage;
    public override float GetSaveValue() => extraDamage;
    public override void SetSaveValue(float v) => extraDamage = v;
    public Buff_BaseValueBuff_ExtraDamage() { buffName = "增伤增益"; }
    public override void BuffEffect(Chess target) { base.BuffEffect(target); target.propertyController.ChangeExtraDamage(extraDamage); }
    public override void BuffOver() { base.BuffOver(); target.propertyController.ChangeExtraDamage(-extraDamage); }
    public override void BuffReset(Buff resetBuff) { base.BuffReset(resetBuff); float v = (resetBuff as Buff_BaseValueBuff_ExtraDamage).extraDamage; if (v > extraDamage) { target.propertyController.ChangeExtraDamage(-extraDamage); extraDamage = v; target.propertyController.ChangeExtraDamage(extraDamage); } }
}

/// <summary>额外暴击伤害</summary>
public class Buff_BaseValueBuff_CritDamage : Buff_BaseValueBuff
{
    public float critDamage;
    public override float GetSaveValue() => critDamage;
    public override void SetSaveValue(float v) => critDamage = v;
    public Buff_BaseValueBuff_CritDamage() { buffName = "暴击伤害增益"; }
    public override void BuffEffect(Chess target) { base.BuffEffect(target); target.propertyController.ChangeCritDamage(critDamage); }
    public override void BuffOver() { base.BuffOver(); target.propertyController.ChangeCritDamage(-critDamage); }
    public override void BuffReset(Buff resetBuff) { base.BuffReset(resetBuff); float v = (resetBuff as Buff_BaseValueBuff_CritDamage).critDamage; if (v > critDamage) { target.propertyController.ChangeCritDamage(-critDamage); critDamage = v; target.propertyController.ChangeCritDamage(critDamage); } }
}

/// <summary>额外护甲</summary>
public class Buff_BaseValueBuff_Armor : Buff_BaseValueBuff
{
    public float armor;
    public override float GetSaveValue() => armor;
    public override void SetSaveValue(float v) => armor = v;
    public Buff_BaseValueBuff_Armor() { buffName = "护甲增益"; }
    public override void BuffEffect(Chess target) { base.BuffEffect(target); target.propertyController.ChangeAR(armor); }
    public override void BuffOver() { base.BuffOver(); target.propertyController.ChangeAR(-armor); }
    public override void BuffReset(Buff resetBuff) { base.BuffReset(resetBuff); float v = (resetBuff as Buff_BaseValueBuff_Armor).armor; if (v > armor) { target.propertyController.ChangeAR(-armor); armor = v; target.propertyController.ChangeAR(armor); } }
}

/// <summary>生命偷取</summary>
public class Buff_BaseValueBuff_LifeSteal : Buff_BaseValueBuff
{
    public float lifeSteal;
    public override float GetSaveValue() => lifeSteal;
    public override void SetSaveValue(float v) => lifeSteal = v;
    public Buff_BaseValueBuff_LifeSteal() { buffName = "生命偷取"; }
    public override void BuffEffect(Chess target) { base.BuffEffect(target); target.propertyController.ChangeLifeSteeling(lifeSteal); }
    public override void BuffOver() { base.BuffOver(); target.propertyController.ChangeLifeSteeling(-lifeSteal); }
    public override void BuffReset(Buff resetBuff) { base.BuffReset(resetBuff); float v = (resetBuff as Buff_BaseValueBuff_LifeSteal).lifeSteal; if (v > lifeSteal) { target.propertyController.ChangeLifeSteeling(-lifeSteal); lifeSteal = v; target.propertyController.ChangeLifeSteeling(lifeSteal); } }
}

/// <summary>体型</summary>
public class Buff_BaseValueBuff_Size : Buff_BaseValueBuff
{
    public int size;
    public override float GetSaveValue() => size;
    public override void SetSaveValue(float v) => size = (int)v;
    public Buff_BaseValueBuff_Size() { buffName = "体型增益"; }
    public override void BuffEffect(Chess target) { base.BuffEffect(target); target.propertyController.ChangeSize(size); }
    public override void BuffOver() { base.BuffOver(); target.propertyController.ChangeSize(-size); }
    public override void BuffReset(Buff resetBuff) { base.BuffReset(resetBuff); int v = (resetBuff as Buff_BaseValueBuff_Size).size; if (v > size) { target.propertyController.ChangeSize(-size); size = v; target.propertyController.ChangeSize(size); } }
}

/// <summary>移速/攻速（ChangeAcceleRate 同时影响）</summary>
public class Buff_BaseValueBuff_AcceleRate : Buff_BaseValueBuff
{
    public float rate;
    public override float GetSaveValue() => rate;
    public override void SetSaveValue(float v) => rate = v;
    public Buff_BaseValueBuff_AcceleRate() { buffName = "加速增益"; }
    public override void BuffEffect(Chess target) { base.BuffEffect(target); target.propertyController.ChangeAcceleRate(rate); }
    public override void BuffOver() { base.BuffOver(); target.propertyController.ChangeAcceleRate(-rate); }
    public override void BuffReset(Buff resetBuff) { base.BuffReset(resetBuff); float v = (resetBuff as Buff_BaseValueBuff_AcceleRate).rate; if (v > rate) { target.propertyController.ChangeAcceleRate(-rate); rate = v; target.propertyController.ChangeAcceleRate(rate); } }
}

public class Buff_BaseValueBuff_TimeValueBuff : TimeBuff
{
    public Buff valueBuff;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.buffController.AddBuff(valueBuff);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        //target.buffController.RemoveBuff(valueBuff);
        target.buffController.buffDic[valueBuff.buffName].BuffOver();
    }
}

public class Buff_BaseValueBuff_UnSelectable : TimeBuff
{
    public Buff_BaseValueBuff_UnSelectable()
    {
        buffName = "无法选中";
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.UnSelectable();
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.ResumeSelectable();
    }
}