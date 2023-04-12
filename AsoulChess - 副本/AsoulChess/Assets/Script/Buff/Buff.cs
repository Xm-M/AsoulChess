using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Buff  
{
    public string buffName;
    public Chess target;
    public virtual void BuffReset()
    {
         
    }
    public virtual void BuffEffect()
    {

    }
    public virtual void BuffOver()
    {

    }
    public virtual Buff Clone()
    {
        return this;
    }
}
