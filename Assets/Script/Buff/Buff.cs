using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Buff  
{
    public string buffName;//这个buff的名字
    public Chess target;//buff的作用对象
    public virtual void BuffReset()
    {
         
    }
    public virtual void BuffEffect( Chess target)
    {
        this.target = target;
    }
    public virtual void BuffOver()
    {
        target.buffController.RemoveBuff(this);
        //GameManage.instance.buffManage.RecycleBuff(this);
    }
    public virtual Buff Clone()
    {
        return new Buff();
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
        base.BuffReset();
        timer.ResetTime();
    }
}
/// <summary>
/// 减速buff 
/// </summary>
public class ColdBuff : TimeBuff
{
    public float slowDownRate;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect( target);
        Debug.Log("减速");
        target.propertyController.ChangeAcceleRate(slowDownRate);
        target.sprite.color = Color.blue;
        timer = GameManage.instance.timerManage.AddTimer(
            BuffOver, continueTime
            );
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeAcceleRate(-slowDownRate);
        target.sprite.color = Color.white;
    }
    public override Buff Clone()
    {
        ColdBuff cold = new ColdBuff();
        cold.buffName = buffName;
        cold.slowDownRate = slowDownRate;
        cold.continueTime = continueTime;
        return cold;
    }
}

public class AccelerateBuff:TimeBuff
{
    public float accelerateRate;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        Debug.Log("开始加速");
        target.propertyController.ChangeAcceleRate(accelerateRate);
        target.sprite.color = Color.red;
        timer = GameManage.instance.timerManage.AddTimer(
            BuffOver, continueTime
            );
    }
    public override void BuffOver()
    {
        base.BuffOver();
        Debug.Log("结束加速");
        target.propertyController.ChangeAcceleRate(-accelerateRate);
        target.sprite.color = Color.white;
    }
    public override Buff Clone()
    {
        AccelerateBuff accelerateBuff = new AccelerateBuff();
        accelerateBuff.accelerateRate = accelerateRate;
        accelerateBuff.continueTime = continueTime;
        accelerateBuff.buffName = buffName;
        return accelerateBuff;
    }
}
