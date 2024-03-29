using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class BuffController:Controller
{
    public Dictionary<string, Buff> buffDic;
    public Chess chess;
    //public UnityEvent buffUpdate;
    public void InitController(Chess chess)
    {
        buffDic = new Dictionary<string, Buff>();
        this.chess = chess;
    }
    public void WhenControllerEnterWar()
    {
         
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
            list.Add( buff.Value);
        }
        for (int i = 0; i < list.Count; i++)
            list[i].BuffOver();
        buffDic.Clear();
    }
    /// <summary>
    /// 添加新的Buff
    /// 如果是已经存在的buff 则重置它 
    /// 否则添加新buff,并激活效果
    /// </summary>
    /// <param name="buffUser"></param>
    /// <param name="buff"></param>
    public void AddBuff(Buff buff)
    {
        if (buffDic.ContainsKey(buff.buffName))
        {
            buffDic[buff.buffName].BuffReset();
        }
        else
        {
            Buff newBuff=GameManage.instance.buffManage.CreateBuff(buff);
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

