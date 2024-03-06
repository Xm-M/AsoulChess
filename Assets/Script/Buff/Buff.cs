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
        GameManage.instance.buffManage.RecycleBuff(this);
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
        target.propertyController.SlowDown(slowDownRate);
        target.GetComponent<SpriteRenderer>().color = Color.blue;
        timer = GameManage.instance.timerManage.AddTimer(
            BuffOver, continueTime
            );
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ResumeSlowDown(slowDownRate);
        target.GetComponent<SpriteRenderer>().color = Color.white;
    }
    public override Buff Clone()
    {
        ColdBuff cold = new ColdBuff();
        cold.buffName = buffName;
        cold.slowDownRate = slowDownRate;
        return cold;
    }
}
