using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuckyBar : PowerBar
{
    float luckyValue;
    public override float GetBarValue()
    {
        return luckyValue;
    }

    public override void InitBar()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateBar(float addValue)
    {
        throw new System.NotImplementedException();
    }
}
