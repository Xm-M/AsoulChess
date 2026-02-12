using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Confusion : State
{
    bool isplay;
    public override void Enter(Chess chess)
    {
        base.Enter(chess);
        isplay = false;
        chess.animatorController.animator.Play("change1" );
        chess.skillController.context.AddEvent(Check);
    }
    public override void Execute(Chess chess)
    {
        base.Execute(chess);
        
    }
    public void Check()
    {
        int mygo = 0;
        chess.skillController.context.TryGet<int>("mygo", out mygo);
        if (mygo > 0&&!isplay)
        {
            isplay = true;
            chess.animatorController.animator.Play("change", 0, 0f);
        }
    }
    public override void Exit(Chess chess)
    {
        base.Exit(chess);
        chess.skillController.context.RemoveEvent(Check);
    }
    public override State Clone()
    {
        Confusion ans = new Confusion();
        ans.stateName = stateName;
        return ans;
    }
}
