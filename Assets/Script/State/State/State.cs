using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public abstract class State 
{
    public StateName stateName;
    public Chess chess;
    
    public virtual void Execute(Chess chess)
    {
    }
    public virtual void Enter(Chess chess)
    {
        
    }
    public virtual void Exit(Chess chess)
    {

    }
    public abstract State Clone();
}
