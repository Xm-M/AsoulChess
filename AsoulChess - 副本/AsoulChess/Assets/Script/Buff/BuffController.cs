using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BuffController 
{
    public Dictionary<string, Buff> buffDic;
    public Chess chess;
    public UnityAction buffUpdate;
    
    public BuffController(Chess c)
    {
        chess = c;
        buffDic = new Dictionary<string,Buff>();
    }
    public void Update()
    {
        if (!GameManage.instance.ifGameStart||chess.ifDeath) return;
        buffUpdate?.Invoke();
    }
    public void LateUpdate()
    {

    }
    public void ResetList()
    {
        foreach (var buff in buffDic)
        {
            buff.Value.BuffOver();
        }
    }
    /// <summary>
    /// 添加新的Buff
    /// 如果是已经存在的buff 则重置它 
    /// 否则添加新buff,并激活效果
    /// </summary>
    /// <param name="buffUser"></param>
    /// <param name="buff"></param>
    public void AddBuff(Chess buffUser,Buff buff)
    {
        if (buffDic.ContainsKey(buff.buffName))
        {
            buffDic[buff.buffName].BuffReset();
        }
        else
        {
            Buff newBuff=buff.Clone();
            buffDic.Add(buff.buffName, newBuff);
            newBuff.target = chess;
            newBuff.BuffEffect();
        }
    }
    public void BuffClear()
    {
        buffDic.Clear();
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
            Debug.LogWarning("û�����buff");
        }
    }
}

