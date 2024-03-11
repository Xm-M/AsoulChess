using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaLacking : Transition
{
     
    public override bool ifReach(Chess chess)
    {
         
        return !chess.skillController.IfUseSkill;
        
    }
    public override Transition Clone()
    {
        return new ManaLacking();
    }
}
