using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IfSkillReadyTransition : Transition
{
    public override Transition Clone()
    {
        return new IfSkillReadyTransition();
    }
    public override bool ifReach(Chess chess)
    {
        //if (chess.skillController.IfSkillReady()) Debug.Log("skill");
        return chess.skillController.IfSkillReady();
    }
}
