using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IWaitTimeTransition : Transition
{
    public float WaitTime;
    float t;
    public override bool ifReach(Chess chess)
    {
        t+=Time.deltaTime;
        if (t < WaitTime)
        {
            return false;
        }
        else
        {
            t = 0;
            return true;
        }
    }
    public override Transition Clone()
    {
        IWaitTimeTransition waitTimeTransition = new IWaitTimeTransition();
        waitTimeTransition.WaitTime = this.WaitTime;
        return waitTimeTransition;
    }
}
