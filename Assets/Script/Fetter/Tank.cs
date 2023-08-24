using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : Fetter
{
    public  float extraHp;
    public Tank(){
        fetterName = "Tank";
    }
    public override void Start()
    {
        base.Start();
        extraHp = 0;
    }
    public override void FetterEffect(int num)
    {
        //self.property.hpMax -= extraHp;
        //Debug.Log("�����");
        switch (num/2)
        {
            case 0:extraHp = 0;break;
            case 1:extraHp = 200;break;
            case 2:extraHp = 400;break;
            case 3:extraHp = 600;break;
        }
        //self.property.hpMax += extraHp;
    }
    public override void FetterReSet()
    {
        //self.property.hpMax -= extraHp;
        extraHp = 0;
    }
    public override void ResetFetter()
    {
        base.ResetFetter();
        
    }
}
