using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuzzyRuleSet : ScriptableObject
{
    public IFuzzyTerm m_pAcendence;
    public IFuzzyTerm m_pConsedence;
    public void SetConfidenceOfConsequentToZero()
    {
        m_pConsedence.ClearDom();
    }
    //该方法计算了前置条件的隶属度，同时计算量后果条件的隶属度
    public void Calculate()
    {
        m_pConsedence.OrWithDom(m_pAcendence.GetDom());
    }
}
