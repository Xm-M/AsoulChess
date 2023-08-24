using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IfAnimPlayOverTransition : Transition
{
    public override bool ifReach(Chess chess)
    {
        return true;
    }
    public override Transition Clone()
    {
        IfAnimPlayOverTransition clone = new IfAnimPlayOverTransition();
        return clone;
    }
}
