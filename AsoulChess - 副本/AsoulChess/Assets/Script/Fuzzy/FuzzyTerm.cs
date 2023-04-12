using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFuzzyTerm
{
    float GetDom();
    void OrWithDom(float val);
    void ClearDom();
}
public class FuzzyAnd : IFuzzyTerm
{
    List<IFuzzyTerm> terms;
    FuzzyAnd(IFuzzyTerm[] term)
    {
        terms = new List<IFuzzyTerm>();
        foreach(var t in term)
        {
            terms.Add(t);
        }
    }
    //返回terms中隶属度的最小值
    public float GetDom()
    {
        float minDom = 0;
        foreach(var t in terms)
        {
            if(t.GetDom() < minDom)minDom = t.GetDom();
        }
        return minDom;
    }

    public void OrWithDom(float val)
    {
        foreach(var t in terms)t.OrWithDom(val); 
    }
    public void ClearDom()
    {
        foreach(var term in terms)term.ClearDom();
    }
}