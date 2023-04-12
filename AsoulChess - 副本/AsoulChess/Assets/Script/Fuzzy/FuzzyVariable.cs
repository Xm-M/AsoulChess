using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFuzzyVariable", menuName = "Fuzzy/FuzzyVariable")]
//模糊语言变量（“距离”），包含多个模糊集合（“{近，中，远}”）
public class FuzzyVariable : ScriptableObject
{
    public string variableName;
    public List<FuzztSet> memberSets;
    public void Fuzzify(float val)
    {
        //分别计算所有模糊集合的隶属度 并将它们保存在各自的m_Dom中
        for(int i = 0; i < memberSets.Count; i++)
        {
            memberSets[i].CalculateDom(val);
        }
    }
    //平均值法去模糊化处理
    public float Defuzzy()
    {
        float confidenceSum = 0; ;
        float valSum = 0;
        for(int i = 0; i < memberSets.Count; i++)
        {
            confidenceSum += memberSets[i].GetDom();
            //Debug.Log("confidence:" + confidenceSum);
            valSum += memberSets[i].GetDom() * memberSets[i].GetRepresentVal();
            //Debug.Log("valsum:" + valSum);
        }
        //Debug.Log("ans:" + valSum / confidenceSum);
        return valSum / confidenceSum;
    }
}
