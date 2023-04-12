using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFuzzySet",menuName = "Fuzzy/FuzzySet")]
//虽然每个都要算 但是每个计算过程应该是独立的吧
//模糊集合基类 这应该是一个抽象类
public class FuzztSet:ScriptableObject,IFuzzyTerm
{
    public string fuzzySetName;
    public float m_Dom;//当前隶属度
    public float m_Max;//隶属度最大值 也就是所谓的代表值
    public Vector3 setRange;
    public Vector3 domRange;
    //计算隶属度
    public void SetRange(Vector3 setRange,Vector3 domRange)
    {
        this.setRange = setRange;
        this.domRange = domRange;
        m_Max = domRange.y;//一般最大值就是中值
    }
    //计算隶属度并将他保存在m_Dom中
    //这个CalculateDom其实就是隶属度函数，把传入的值计算隶属度（模糊化）
    //要重写的话一般都是在这
    public virtual float CalculateDom(float val)
    {
        if (val > setRange.y && val < setRange.z)
        {
            float k = (domRange.y - domRange.z) / (setRange.y - setRange.z);
            m_Dom = k * (val - setRange.y) + domRange.y;
        }
        else if (val <= setRange.y && val > setRange.x)
        {
            float k = (domRange.y - domRange.x) / (setRange.y - setRange.x);
            m_Dom = k * (val - setRange.y) + domRange.y;
        }
        else m_Dom = 0;
        //Debug.Log(fuzzySetName+":"+ m_Dom);
        return m_Dom;
    }
    //合成结论
    //若该集合为结果集合 并且符合一条规则 将其隶属度设置为置信度
    public virtual void OrWithDom(float val)
    {
        if (m_Dom < val) m_Dom = val;
    }
    public float GetRepresentVal()
    {
        if (domRange.x == domRange.y) return (setRange.x + setRange.y) / 2;
        if (domRange.z == domRange.y) return (setRange.z + setRange.y) / 2;
        return setRange.y;
    }
    public virtual float GetDom()
    {
        return m_Dom;
    }
    public virtual void ClearDom()
    {
        m_Dom = 0;
    }
}
