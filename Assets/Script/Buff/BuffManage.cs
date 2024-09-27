using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManage : IManager
{
    public Dictionary<string, Stack<Buff>> buffDic;
    public void InitManage()
    {
         buffDic = new Dictionary<string, Stack<Buff>>();
    }

    public void OnGameOver()
    {
        buffDic.Clear();
    }

    public void OnGameStart()
    {
        buffDic.Clear();   
    }
    public Buff CreateBuff(Buff buff)
    {
        if (buffDic.ContainsKey(buff.buffName))
        {
            if (buffDic[buff.buffName].Count > 0)
            {
                return buffDic[buff.buffName].Pop();
            }
            else
            {
                return buff.Clone();
            }
        }
        else
        {
            buffDic.Add(buff.buffName, new Stack<Buff>());
            return buff.Clone();
        }
    }
    public void RecycleBuff(Buff buff)
    {
        if (buffDic.ContainsKey(buff.buffName))
        {
            buffDic[buff.buffName].Push(buff);
        }
        else
        {
            Debug.LogWarning("û�����buff"+buff.buffName);
        }
    }
    public void Clear()
    {
        Debug.Log("����buffϵͳ");
        buffDic.Clear();

    }
}
