using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition_ZombieKing_ChangeIdleTransition : Transition
{
    public float changeStateTime=10;
    float t = 0;
    bool stand = true;
    public override bool ifReach(Chess chess)
    {
        t += Time.deltaTime;
        if (t >= changeStateTime)
        {
            t = 0;
            stand = !stand;
            chess.skillController.context.Set<bool>("stand",stand);
            return true; 
        }
        return false;
    }

    public override Transition Clone()
    {
        Transition clone = new Transition_ZombieKing_ChangeIdleTransition();
        return clone;
    }
}
