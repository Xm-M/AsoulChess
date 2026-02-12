using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animator_MMK : AnimatorController
{
    public PropertyCreator ninaCreator;
    bool nina;

    public override void WhenControllerEnterWar()
    {
        nina = false;
        foreach (var chess in GameManage.instance.chessTeamManage.GetTeam(chess.tag))
        {
            if (chess.propertyController.creator == ninaCreator)
            {
                nina = true;
            }
        }
        //if (nina) Debug.Log("捕捉到了");
        //else Debug.Log("没捕捉到");
        EventController.Instance.AddListener<Chess>(EventName.WhenChessEnterWar.ToString(),CheckNima);
        EventController.Instance.AddListener<Chess>(EventName.WhenDeath.ToString(), CheckNinaOut);
        PlayIdle();
    }
    public override void WhenControllerLeaveWar()
    {
        base.WhenControllerLeaveWar();
        EventController.Instance.RemoveListener<Chess>(EventName.WhenChessEnterWar.ToString(), CheckNima);
        EventController.Instance.RemoveListener<Chess>(EventName.WhenDeath.ToString(),CheckNinaOut);
    }
    public void CheckNima(Chess c)
    {
        if (!nina&&c.CompareTag(chess.tag) && c.propertyController.creator == ninaCreator)
        {
            nina = true;
            if (chess.stateController.currentState.state.stateName == StateName.IdleState)
            {
                animator.Play("change");
            }
        }
    }

    public void CheckNinaOut(Chess c)
    {
        if (nina&& c.CompareTag(chess.tag) && c.propertyController.creator == ninaCreator)
        {
            foreach (var chess in GameManage.instance.chessTeamManage.GetTeam(chess.tag))
            {
                if (chess.propertyController.creator == ninaCreator&&chess!=c)
                {
                    return;
                }
            }
        
            nina = false;
            if (chess.stateController.currentState.state.stateName == StateName.IdleState)
            {
                animator.Play("change2");
            }
        }
    }
    public override void PlayIdle()
    {
        if (nina)
        {
            animator.Play("idle2");
        }
        else
        {
            animator.Play("idle");
        }
    }
    public override void PlaySkill()
    {
        //base.PlaySkill();
        if (nina)
        {
            animator.Play("skill2");
        }
        else
        {
            animator.Play("skill");
        }
    }
    public override void PlayAttack()
    {
        if (nina)
        {
            animator.Play("attack2");
        }
        else
        {
            animator.Play("attack");
        }
    }
    
}
