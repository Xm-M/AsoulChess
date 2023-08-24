using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Buff  
{
    public string buffName;
    public Chess target;
    public Chess buffFrom;
    public virtual void BuffReset()
    {
         
    }
    public virtual void BuffEffect(Chess buffFrom,Chess target)
    {
        this.target = target;
        this.buffFrom = buffFrom;
    }
    public virtual void BuffOver()
    {

    }
    public virtual Buff Clone()
    {
        return this;
    }
}
public class TimeBuff : Buff
{
    public float continueTime;
    float t;
    public override void BuffReset()
    {
        base.BuffReset();
        t = 0;
    }
    public override void BuffEffect(Chess buffFrom, Chess target)
    {
        base.BuffEffect(buffFrom, target);
        target.buffController.buffUpdate.AddListener(TimeAdd);
        t = 0;
    }
    public override void BuffOver()
    {
        base.BuffOver();
    }

    public void TimeAdd()
    {
        t += Time.deltaTime;
        if (t > continueTime)
        {
            target.buffController.RemoveBuff(buffName);
            target.buffController.buffUpdate.RemoveListener(TimeAdd);
        }
    }
}
/// <summary>
/// ¼õËÙbuff 
/// </summary>
public class ColdBuff : TimeBuff
{
    public float slowDownRate;
    public override void BuffEffect(Chess buffFrom, Chess target)
    {
        base.BuffEffect(buffFrom, target);
        target.propertyController.SlowDown(slowDownRate);
        target.GetComponent<SpriteRenderer>().color = Color.blue;
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
