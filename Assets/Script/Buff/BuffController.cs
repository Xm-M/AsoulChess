using System.Collections.Generic;
using System;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

[Serializable]
public class BuffController
{
#if UNITY_EDITOR
    [Serializable]
    public class BuffStateDisplay
    {
        public string buffName;
    }

    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isEditor && UnityEngine.Application.isPlaying")]
    private List<BuffStateDisplay> BuffStates => GetBuffStatesForEditor();

    private List<BuffStateDisplay> GetBuffStatesForEditor()
    {
        var list = new List<BuffStateDisplay>();
        if (buffDic == null) return list;
        foreach (var kv in buffDic)
        {
            list.Add(new BuffStateDisplay
            {
                buffName = kv.Key
            });
        }
        return list;
    }
#endif
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
        if (buff == null) return;
        if (buffDic.ContainsKey(buff.buffName))
        {
            buffDic[buff.buffName].BuffReset(buff);
            //Debug.Log(chess.name+ " 重置了" + buff.buffName);
        }
        else
        {
            //Debug.Log(chess.name+"获得了" + buff.buffName);
            Buff newBuff = buff.Clone();
            newBuff.buffName = buff.buffName;
            buffDic.Add(buff.buffName, newBuff);
            newBuff.BuffEffect(chess);
        }
    }
    public void TryOverBuff(Buff buff)
    {
        if(buff == null) return;
        if (buffDic.ContainsKey(buff.buffName))
        {
            buffDic[buff.buffName].BuffOver();
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
            Debug.Log("没有这个buff");
        }
    }

    /// <summary>
    /// 采集 Buff 存档数据，跳过纯 Buff_BaseValueBuff
    /// </summary>
    public List<BuffSaveData> GetSaveData()
    {
        var list = new List<BuffSaveData>();
        if (buffDic == null) return list;
        foreach (var kv in buffDic)
        {
            if (kv.Value is Buff_BaseValueBuff) continue;
            var data = new BuffSaveData();
            kv.Value.WriteToSaveData(data);
            list.Add(data);
        }
        return list;
    }

    /// <summary>
    /// 读档时恢复 Buff：从注册表取类型 → Create → RestoreFromSaveData → AddBuff
    /// </summary>
    public void AddBuffFromSave(BuffSaveData data)
    {
        if (data == null || string.IsNullOrEmpty(data.id)) return;
        var buffType = BuffDatabase.GetBuffType(data.id) ?? data.buffType;
        if (string.IsNullOrEmpty(buffType)) return;
        var template = BuffFactory.Create(buffType);
        if (template == null) return;
        var clone = template.Clone();
        clone.buffName = data.id;
        clone.RestoreFromSaveData(data);
        AddBuff(clone);
    }
}
