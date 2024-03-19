using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public abstract class Transition 
{
    public virtual bool ifReach(Chess chess)
    {
        return false;
    }
    public abstract Transition Clone();
}
