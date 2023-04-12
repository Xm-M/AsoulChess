using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InRangeTransition : Transition
{
    public InRangeTransition(){
        transitionName=TransitionName.InRangeTransition;
    }
    public override bool ifReach(Chess chess)
    {
        if (Vector2.Distance(chess.transform.position,chess.standTile.transform.position)<0.1f
            &&chess.target&&chess.target.gameObject.activeSelf&&MapManage.instance.Distance(chess.standTile, chess.target.standTile) <= chess.propertyController.Data.attacRange)
        {
            return true;
        }
        return false;
    }
    public override Transition Clone()
    {
        return new InRangeTransition();
    }
}
