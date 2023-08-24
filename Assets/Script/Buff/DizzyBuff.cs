using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DizzyBuff : Buff
{
    public float dizzyTime;
    Timer timer;
    public override void BuffEffect(Chess buffFrom, Chess target)
    {
        base.BuffEffect(buffFrom, target);
        target.stateController.ChangeState(StateName.DizzyState);
        this.timer= GameManage.instance.timerManage.AddTimer(BuffOver, dizzyTime);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.stateController.RevertToPreState();
    }
    public override void BuffReset()
    {
        base.BuffReset();
        timer.ResetTime();
    }
    public override Buff Clone()
    {
        DizzyBuff dizzyBuff = new DizzyBuff();
        dizzyBuff.dizzyTime = dizzyTime;
        return dizzyBuff;
    }
}
public class AirborneBuff : DizzyBuff
{
    public override void BuffEffect(Chess buffFrom, Chess target)
    {
        base.BuffEffect(buffFrom, target);
    }
    public override void BuffOver()
    {
        base.BuffReset();
    }
}
