using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animator_Mygo : AnimatorController
{
    bool friend;
    //float value;
    public override void WhenControllerEnterWar()
    {
        base.WhenControllerEnterWar();
        //value = -1f;
        friend = false;
    }
    public override void ChangeFloat(string vname, float value)
    {
        //if (this.value == value) return;
        //this.value = value;
        base.ChangeFloat(vname, value);
        //Debug.Log(chess);
        if (vname=="Mygo")
        {
            if (value > 0 && !friend)
            {
                //Debug.Log("成员大于1"+chess.moveController.standTile.mapPos);
                if(chess.stateController.currentState.state != null && chess.stateController.currentState.state.stateName == StateName.IdleState)
                    animator.Play("change");
                friend = true;
            }
            else if (value < 0.1f && friend)
            {
                if(chess.stateController.currentState.state != null && chess.stateController.currentState.state.stateName == StateName.IdleState)
                    animator.Play("change1");
                //Debug.Log("成员小于1");
                friend = false;

            }
        }
    }
    /// <summary>
    /// 其实主要就是idle的问题对吧
    /// </summary>
    public override void PlayIdle()
    {
        //base.PlayIdle();
        float mygo = animator.GetFloat("Mygo");
        if (mygo<1)
        {
            animator.Play("idle1");
        }
        else if (mygo>0.9)
        {
            animator.Play("idle2");
        }
    }
}
