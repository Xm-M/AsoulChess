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
        return chess.equipWeapon.IfInRange();
    }
    public override Transition Clone()
    {
        return new InRangeTransition();
    }
}
