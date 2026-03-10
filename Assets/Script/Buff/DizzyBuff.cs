using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这个是dizzyBuff 那dizzynessBuff怎么办呢
/// </summary>
public class DizzyBuff : Buff
{
    public float dizzyTime;
    protected Timer timer;
    public override void WriteToSaveData(BuffSaveData data)
    {
        base.WriteToSaveData(data);
        if (data != null && timer != null) data.remainingTime = timer.LeftTime();
    }
    public override void RestoreFromSaveData(BuffSaveData data)
    {
        base.RestoreFromSaveData(data);
        if (data != null && data.remainingTime >= 0) _restoreRemainingTime = data.remainingTime;
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.stateController.ChangeState(StateName.DizzyState);
        float duration = _restoreRemainingTime >= 0 ? _restoreRemainingTime : dizzyTime;
        _restoreRemainingTime = -1f;
        this.timer = GameManage.instance.timerManage.AddTimer(BuffOver, duration);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.stateController.RevertToPreState();
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        dizzyTime = Mathf.Max((resetBuff as DizzyBuff).dizzyTime, dizzyTime);
        timer.ResetTime(dizzyTime);
    }
    public override Buff Clone()
    {
        DizzyBuff dizzyBuff = new DizzyBuff();
        dizzyBuff.dizzyTime = dizzyTime;
        return dizzyBuff;
    }
}
/// <summary>
/// 这tm是个啥
/// </summary>
public class AirborneBuff : DizzyBuff
{
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
    }
    public override void BuffOver()
    {
        base.BuffReset(this);
    }
}
public class DizznessBuff : TimeBuff
{
    //StateName current;
    public GameObject Dizznesseffect;
    GameObject effect;
    public override void BuffEffect(Chess target)
    {
        //continueTime = target.propertyController.GetDizznessTime();
        base.BuffEffect(target);
        target.propertyController.ChangeDizznessTime(continueTime);
        if (Dizznesseffect != null)
        {
            effect = ObjectPool.instance.Create(Dizznesseffect);
            effect.transform.SetParent(target.transform);
            effect.transform.localPosition = Vector3.zero;
        }

    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        continueTime = (resetBuff as DizznessBuff).continueTime;
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