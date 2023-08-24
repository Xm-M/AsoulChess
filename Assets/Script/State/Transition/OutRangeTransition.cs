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
         
        return !chess.equipWeapon.IfInRange();
    }
    public override Transition Clone()
    {
        return new OutRangeTransition();
    }
}
