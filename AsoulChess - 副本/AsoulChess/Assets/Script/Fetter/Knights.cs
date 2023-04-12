using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knights : Fetter
{
    public  float extraArmor;
    public Knights(){
        fetterName = "Knights";
    }
    public override void Start()
    {
        base.Start();
        
        extraArmor = 0;
    }
    public override void FetterEffect(int num)
    {
        switch (num-1)
        {
            case 0: extraArmor = 0; break;
            case 1: extraArmor = 20; break;
            case 2: extraArmor = 40; break;
            case 3: extraArmor = 60; break;
        }
        //self.property.armorCurrent += extraArmor;
        //Debug.Log("���⻤��"+ extraArmor+" " + self.name+" "+ self.property.armorCurrent);
    }
    public override void FetterReSet()
    {
        //self.property.armorCurrent -= extraArmor;
        extraArmor = 0;
    }
    public override void ResetFetter()
    {
        base.ResetFetter();
    }
}
