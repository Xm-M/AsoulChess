using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wild : Fetter
{
    public float extraHp;
    public float extraAttackSpeed;
    public Wild(){
        fetterName="Wild";
    }
    public override void Start()
    {
        base.Start();
        
    }
    public override void FetterEffect(int num)
    {
        base.FetterEffect(num);
    }
    public override void ResetFetter()
    {
        base.ResetFetter();
    }
}
