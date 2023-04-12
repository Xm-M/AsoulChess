using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPYFAMILY : Fetter
{
    float extraattack;
    float extraHp;
    float extraMP;
    float extraArmor;
    public override void Start()
    {
        base.Start();
        fetterName = "SPYFAMILY";
    }
    public override void FetterEffect(int num)
    {
        Debug.Log("������� "+num);
        ResetProperty();
        if (num >= 1)
        {
            extraattack=40;
            extraHp=400;
            extraMP=40;
            extraArmor=40;
        }
        else
        {
            extraattack = 0;
            extraHp = 0;
            extraMP = 0;
            extraArmor = 0;
        }
        SetProperty();
    }
    public void ResetProperty()
    {

    }
    public void SetProperty()
    {

    }
    public override void FetterReSet()
    {
        ResetProperty();
        base.FetterReSet();
    }
}
