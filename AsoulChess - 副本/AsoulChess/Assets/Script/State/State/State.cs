using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State 
{
    public StateName stateName;
    public Chess chess;
    
    public virtual void Excute(Chess chess)
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
