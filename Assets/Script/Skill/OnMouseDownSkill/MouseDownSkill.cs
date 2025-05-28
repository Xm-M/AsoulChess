using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
public class MouseDownSkill : MonoBehaviour
{
    public bool IfDown { get; private set; }
    //public int minue;
    //public int seccon;
    //public int allsceon;
    //[Button]
    //public void Button()
    //{
    //    allsceon = minue * 60 + seccon;
    //}
    protected virtual void OnMouseDown()
    {
        IfDown = true;
        Debug.Log("use");
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
