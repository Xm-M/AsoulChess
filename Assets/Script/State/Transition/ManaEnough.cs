using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaEnough : Transition
{
    public ManaEnough(){
        transitionName=TransitionName.ManaEnough;
    }
    public override bool ifReach(Chess chess)
    {
        return chess.skillController.IfUseSkill;
    }
    public override Transition Clone()
    {
        ManaEnough ans=new ManaEnough();
        return ans;
    }
}
