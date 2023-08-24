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
    public UnityEvent buffUpdate;
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
        buffUpdate.RemoveAllListeners();
    }
    public void Update()
    {
        if (!GameManage.instance.ifGameStart||chess.IfDeath) return;
        buffUpdate?.Invoke();
    }
    public void ResetList()
    {
        foreach (var buff in buffDic)
        {
            buff.Value.BuffOver();
        }
        buffDic.Clear();
    }
    /// <summary>
    /// 添加新的Buff
    /// 如果是已经存在的buff 则重置它 
    /// 否则添加新buff,并激活效果
    /// </summary>
    /// <param name="buffUser"></param>
    /// <param name="buff"></param>
    public void AddBuff(Chess buffFrom,Buff buff)
    {
        if (buffDic.ContainsKey(buff.buffName))
        {
            buffDic[buff.buffName].BuffReset();
        }
        else
        {
            Buff newBuff=buff.Clone();
            buffDic.Add(buff.buffName, newBuff);
            newBuff.BuffEffect(buffFrom,chess);
        }
    }
    public void RemoveBuff(string buffName)
    {
        Debug.Log("removeBuff");
        if (buffDic.ContainsKey(buffName))
        {
            buffDic[buffName].BuffOver();
            ObjectPool.instance.ReycleObject(buffDic[buffName]);
            buffDic.Remove(buffName);
        }
        else
        {
            Debug.LogWarning("没有这个buff");
        }
    }

    
}

