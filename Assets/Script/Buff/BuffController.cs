using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class BuffController
{
    public Dictionary<string, Buff> buffDic;
    public Chess chess;
    [SerializeReference]
    public List<Buff> onEnterBuff;
    //public UnityEvent buffUpdate; 
    public void InitController(Chess chess)
    {
        buffDic = new Dictionary<string, Buff>();
        this.chess = chess; 
        //onEnterBuff = new List<Buff>(); 
    }
    public void WhenControllerEnterWar()
    {
        for (int i = 0; i < onEnterBuff.Count; i++)
        {
            AddBuff(onEnterBuff[i]);
        }
    }

    public void WhenControllerLeaveWar()
    {
        ResetList();
    }
    public void ResetList()
    {
        List<Buff> list = new List<Buff>();
        foreach (var buff in buffDic)
        {
            list.Add(buff.Value);
        }
        for (int i = 0; i < list.Count; i++)
            list[i].BuffOver();
        buffDic.Clear();
    }
    /// <summary> 
    /// 添加新的Buff  
    /// 如果是已经存在的buff 则重置它 
    /// 否则添加新buff,并激活效果      
    ///  买不起不至于 主要是不喜欢魂类 
    /// </summary>
    /// <param name="buffUser"></param>
    /// <param name="buff"></param>
    public void AddBuff(Buff buff)
    {

        if (buffDic.ContainsKey(buff.buffName))
        {
            buffDic[buff.buffName].BuffReset();
            Debug.Log("重置了" + buff.buffName);
        }
        else
        {
            Debug.Log("获得了" + buff.buffName);
            Buff newBuff = buff.Clone();
            newBuff.buffName = buff.buffName;
            buffDic.Add(buff.buffName, newBuff);
            newBuff.BuffEffect(chess);
        }
    }
    public void RemoveBuff(Buff buff)
    {
        if (buffDic.ContainsKey(buff.buffName))
        {
            buffDic.Remove(buff.buffName);
        }
        else
        {
            Debug.LogWarning("没有这个buff");
        }
    }


}