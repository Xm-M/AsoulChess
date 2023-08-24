using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IfDeathTransition : Transition
{
    public override bool ifReach(Chess chess)
    {
        return chess.propertyController.GetHp() <= 0;
    }
    public override Transition Clone()
    {
        IfDeathTransition transition = new IfDeathTransition();
        return transition;
    }
}
