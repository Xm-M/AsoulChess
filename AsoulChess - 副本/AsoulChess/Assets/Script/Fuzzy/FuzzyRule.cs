using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFuzzyRule", menuName = "Fuzzy/FuzzyRule")]
public class FuzzyRule:ScriptableObject
{
    public FuzztSet m_pAntecedents;//前提
    public FuzztSet m_pConsequence;//结果
    //模糊规则的任务就是将传入的前提计算 之后算出后果的置信度
    
    public void ClearConsequence()
    {
        m_pConsequence.ClearDom();
    }
    public void Calculate()
    {
        //更新后果条件的隶属度
        m_pConsequence.OrWithDom(m_pAntecedents.GetDom());
    }
}
