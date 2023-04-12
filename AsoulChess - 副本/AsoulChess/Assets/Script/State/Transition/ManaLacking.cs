using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaLacking : Transition
{
    public ManaLacking(){
        transitionName=TransitionName.ManaLacking;
    }
    public override bool ifReach(Chess chess)
    {
         
        return chess.skillController.skillOver;
        
    }
    public override Transition Clone()
    {
        return new ManaLacking();
    }
}
