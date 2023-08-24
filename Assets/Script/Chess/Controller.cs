using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Controller 
{
    public void InitController(Chess chess);
    public void WhenControllerEnterWar();
    public void WhenControllerLeaveWar();
}
