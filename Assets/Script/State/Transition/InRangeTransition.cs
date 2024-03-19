using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InRangeTransition : Transition
{
     
    public override bool ifReach(Chess chess)
    {
        return chess.equipWeapon.IfFindEnemy();
    }
    public override Transition Clone()
    {
        return new InRangeTransition();
    }
}
