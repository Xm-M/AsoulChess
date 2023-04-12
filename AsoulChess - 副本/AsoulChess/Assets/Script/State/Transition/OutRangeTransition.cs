using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutRangeTransition : Transition
{
    public OutRangeTransition(){
        transitionName=TransitionName.OutRangeTransition;
    }
    public override bool ifReach(Chess chess)
    {
        if (!chess.target||chess.target.unSelectable||!chess.target.gameObject.activeSelf||MapManage.instance.Distance(chess.standTile, chess.target.standTile) > chess.propertyController.Data.attacRange)
        {
            return true;
        }
        return false;
    }
    public override Transition Clone()
    {
        return new OutRangeTransition();
    }
}
