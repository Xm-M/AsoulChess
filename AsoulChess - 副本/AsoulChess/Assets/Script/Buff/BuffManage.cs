using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManage : MonoBehaviour
{
    [SerializeField]List<Buff> buffs;
    Dictionary<string,Buff> buffDic;
    void Awake()
    {
        buffDic=new Dictionary<string, Buff>();
        foreach(var buff in buffs){
            buffDic.Add(buff.buffName,buff);
        }
    }
    public Buff GetBuff(string buffName){
        return buffDic[buffName];
    }
}
