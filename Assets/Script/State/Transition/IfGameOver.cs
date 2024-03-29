using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IfGameOver : Transition
{
    public override bool ifReach(Chess chess)
    {
        if (LevelManage.instance.IfGameStart)
            return false;
        return true;
    }
    public override Transition Clone()
    {
        return new IfGameOver();
    }
}
