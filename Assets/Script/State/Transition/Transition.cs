using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public abstract class Transition 
{
    public TransitionName transitionName;
    public virtual bool ifReach(Chess chess)
    {
        return false;
    }
    public abstract Transition Clone();
}