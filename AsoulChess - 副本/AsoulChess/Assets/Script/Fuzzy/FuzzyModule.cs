using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuzzyModule : MonoBehaviour
{
    //每个ai都拥有一个fzm实例
    //通过名字访问模糊变量
    public string moduleName;
    //模糊语言变量的list 
    public List<FuzzyVariable> VariableList; 
    //一个描述对应一个模糊语言变量 比如“距离”={近，中，远}；“期望”={低，中，高}；
    public Dictionary<string, FuzzyVariable> varMap;
    //包含了该模块下的所有模糊规则
    public List<FuzzyRule> fuzzyRules;
    //用于创建一个模糊变量
    private void Start()
    {

    }
    public void ClearAllConsequents()
    {
        for (int i = 0; i < fuzzyRules.Count; i++)
        {
            fuzzyRules[i].ClearConsequence();
        }
    }
    //调用指定模糊语言变量的模糊化方法,这个是模糊推理的第一步
    //例：Fuzzify("距离",5);然后
    public void Fuzzify(string flvName,float val)
    {
        if (!varMap.ContainsKey(flvName)) Debug.LogError("没有这个模糊变量");
        else varMap[flvName].Fuzzify(val);//计算隶属度 然后把他们放在各自的dom中
    }
    //给定模糊变量 用去模糊方法来去模糊化
    //去模糊化应该是最后一步了，一般是最后算值的时候用的了
    public float Defuzzy(string key)
    {
        if (!varMap.ContainsKey(key)) Debug.LogError("没有这个模糊变量2");
        //清除所有后果的隶属度
        ClearAllConsequents();
        //这是模糊推理的第二步
        for(int i = 0; i < fuzzyRules.Count; i++)
        {
            fuzzyRules[i].Calculate();
        }
        //这是模糊推理的第三步
        float defuzzy = varMap[key].Defuzzy();
        //Debug.Log("最后得到的期望值为：" + defuzzy);
        return defuzzy;
    }

}
