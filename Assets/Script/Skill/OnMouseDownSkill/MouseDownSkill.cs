using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MouseDownSkill : MonoBehaviour
{
    public bool IfDown { get; private set; }
    protected virtual void OnMouseDown()
    {
        IfDown = true;    
    }
    protected virtual void OnMouseUp()
    {
        IfDown=false;
    }
    protected virtual void OnMouseExit()
    {
        IfDown = false;
    }
}
