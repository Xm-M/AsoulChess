using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBuff : Buff
{
    public float continueTime;//����ʱ��
    public float interval;//���ʱ��
    float useTime;
    float t;
    public override void BuffEffect()
    {
        base.BuffEffect();
        useTime=continueTime;
        target.buffController.buffUpdate += BuffUpdate;
    }
    public override void BuffReset()
    {
        base.BuffReset();
        useTime = continueTime;
    }
    public virtual void BuffUpdate()
    {
        if (useTime > 0)
        {
            useTime -= Time.fixedDeltaTime;
            t += Time.fixedDeltaTime;
            if (t > interval)
            {
                t = 0;
                ExtraEffect();
            }
        }
    }
    public virtual void ExtraEffect()
    {

    }

}
