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
        //if (chess.skill == null) return false;
        //if (chess.skill.ManaCost>0&& chess.property.GetValue(ValueType.Mana).currentValue >= chess.skill.ManaCost)
        //{
        //    Debug.Log(chess.skill.ManaCost+" "+chess.property.GetValue(ValueType.Mana).currentValue);
        //    return true;
        //}
        return false;
    }
    public override Transition Clone()
    {
        ManaEnough ans=new ManaEnough();
        return ans;
    }
}
