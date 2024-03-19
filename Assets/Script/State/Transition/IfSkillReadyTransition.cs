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
        return chess.skillController.IfSkillReady();
    }
}
