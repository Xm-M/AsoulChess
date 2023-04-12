using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FSPrice", menuName = "Fuzzy/FSPrice")]
public class FSPrice : FuzztSet
{
    public override float CalculateDom(float val)
    {
        float priceDis =Mathf.Abs(MoneyManage.instance.teamM["Enemy"].currenMoney - val);
        return base.CalculateDom(priceDis);
    }
}
