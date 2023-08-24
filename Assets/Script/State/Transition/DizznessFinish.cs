using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DizznessFinish : Transition
{
    public override bool ifReach(Chess chess)
    {
        if (true) return false;
        return true;
    }
    public override Transition Clone()
    {
        return new DizznessFinish();
    }
}
