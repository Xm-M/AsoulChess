using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IfGameStart : Transition
{
    public override bool ifReach(Chess chess)
    {
        if (GameManage.instance.ifGameStart )
            return true;
        return false;
    }
    public override Transition Clone()
    {
        return new IfGameStart();
    }
}
