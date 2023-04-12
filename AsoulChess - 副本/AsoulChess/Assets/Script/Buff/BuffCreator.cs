using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="BuffCreator",menuName ="BuffCreator")]
public class BuffCreator : ScriptableObject
{
    public string buffName;
    [SerializeReference]
    public Buff buff;
    public Buff CreateBuff()
    {
        return buff.Clone();
    }
    public void Refresh()
    {
        if (buff==null||buff.GetType().ToString() != buffName)
        {
            buff = Activator.CreateInstance(Type.GetType(buffName)) as Buff;
        }
    }
}
