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