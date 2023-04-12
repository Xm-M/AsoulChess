using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFuzzySetAnd", menuName = "Fuzzy/FuzzySetAnd")]
public class FuzzySetAnd : FuzztSet
{
    public List<FuzztSet> fuzzySets = new List<FuzztSet>();
    public override void OrWithDom(float val)
    {
        foreach (FuzztSet set in fuzzySets)
        {
            set.OrWithDom(val);
        }
    }
    public override float GetDom()
    {
        float minDom = 0;
        foreach(FuzztSet set in fuzzySets)
        {
            if(set.GetDom() < minDom)minDom = set.GetDom();
        }
        return minDom;
    }
    public override void ClearDom()
    {
        foreach(FuzztSet set in fuzzySets)set.ClearDom();
    }
}
