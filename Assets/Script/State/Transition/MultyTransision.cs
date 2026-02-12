using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultyTransision_And : Transition
{
    [SerializeReference]
    public List<Transition> transitions;
    public override bool ifReach(Chess chess)
    {
        bool ans=true;
        foreach(var transition in transitions)
        {
            ans=ans && transition.ifReach(chess);
        }
        return ans;
    }
    public override Transition Clone()
    {
        MultyTransision_And multyTransision = new MultyTransision_And();
        multyTransision.transitions = new List<Transition>();
        foreach(var transition in this.transitions)
        {
            multyTransision.transitions.Add(transition);
        }
        return multyTransision;
    }
}
