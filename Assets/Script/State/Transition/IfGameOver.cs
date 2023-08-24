using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IfGameOver : Transition
{
    public override bool ifReach(Chess chess)
    {
        if (GameManage.instance.ifGameStart )
            return false;
        return true;
    }
    public override Transition Clone()
    {
        return new IfGameOver();
    }
}
