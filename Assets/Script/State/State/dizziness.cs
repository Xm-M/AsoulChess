using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 眩晕状态 具体的话应该有个眩晕标志
/// </summary>
public class DizzinessState : State
{
    float t;
    //StateName preState;
    //bool dizznessOver;
    public DizzinessState()
    {
        stateName=StateName.DizzyState;
    }
    public override void Enter(Chess chess)
    {
        base.Enter(chess);
        t = 0;
        chess.animatorController.PlayDizzy();
        //dizznessOver = false;
        //Debug.Log("进入眩晕状态");
        //Debug.Log(chess.propertyController.GetDizznessTime());
    }
    public override void Execute(Chess chess)
    {
        t+=Time.deltaTime;
        if (t > chess.propertyController.GetDizznessTime())
        {
            t = 0;
            chess.RevertPreState();
        }
    }
    public override void Exit(Chess chess)
    {
        base.Exit(chess);
        chess.propertyController.ResetDizznessTime();
        t = 0;
    }
    public override State Clone()
    {
        return new DizzinessState();
    }
}
