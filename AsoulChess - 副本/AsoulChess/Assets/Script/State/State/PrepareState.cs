using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PrepareState : State
{
    public PrepareState(){
        stateName=StateName.PrepareState;
    }
    public override State Clone()
    {
        PrepareState ans= new PrepareState();
        ans.stateName=stateName;
        return ans;
    }
}
