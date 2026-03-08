using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Shake : EffectMiss 
{
    public float duration=0.3f;
    public float strength=0.1f;
    protected override void Start()
    {
        base.Start();
        GameManage.instance.cameraManage.Shake(duration,strength);
    }
}
