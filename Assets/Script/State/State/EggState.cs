using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggState : State
{
    public EggState()
    {
        stateName=StateName.EggState;
    }
    public override void Enter(Chess chess)
    {
        //好像什么都不用做啊 哦还是要的 触发菜单期间不可选中
        chess.UnSelectable();
        base.Enter(chess);
    }
    public override void Exit(Chess chess)
    {
        base.Exit(chess);
        chess.ResumeSelectable();
    }
    public override State Clone()
    {
        //throw new System.NotImplementedException();
        EggState clone = new EggState();
        return clone;
    }
}
