using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
/// <summary>
/// 这个是菜单控制器 
/// </summary>
public class EggController : MonoBehaviour
{
    public Chess user;
    //[SerializeReference]
    [LabelText("彩蛋列表")]
    public List<string> eggsNames;
    public List<HideEgg> hideEggs;
    int currentEgg;
    public AudioPlayer player;
    public void InitEggs(Chess user)
    {
        hideEggs = new List<HideEgg>();
        if (eggsNames.Count == 0) return;
        foreach(var eggname in eggsNames)
        {
            Type t = Type.GetType(eggname);
            HideEgg egg= Activator.CreateInstance(t) as HideEgg;
            hideEggs.Add(egg);
        }
        this.user = user;
        foreach(var egg in hideEggs)
        {
            egg.InitEgg(user);
        }
    }
    public void WhenEnterWar()
    {
        foreach(var egg in hideEggs)
        {
            egg.ifTrigger = false;
            egg.WhenEnterWar(user);
        }
    }
    public void WhenLeaveWar()
    {
        foreach(var egg in hideEggs)
        {
            egg.WhenLeaveWar(user);
        }
    }
    public bool CheckEgg(Chess target)
    {
        for(int i = 0; i < hideEggs.Count; i++)
        {
            if (!hideEggs[i].ifTrigger &&hideEggs[i].IfTriggerEgg(user,target))
            {
                currentEgg = i;
                hideEggs[i].ifTrigger = true;
                //Debug.Log(currentEgg);
                return true;
            }
        }
        return false;
    }
    public void UseEggEffect()
    {
        hideEggs[currentEgg].WhenEggOver();
    }
}
public abstract class HideEgg
{
    public string targetName;
    public Chess self;
    public Chess target;
    public bool ifTrigger;
    public abstract void InitEgg(Chess user);
    public abstract void WhenEnterWar(Chess user);
    public abstract void WhenLeaveWar(Chess user);
    public abstract bool IfTriggerEgg(Chess chess, Chess target);
    public abstract void TriggerEggEffect(Chess chess, Chess target);
    public abstract void WhenEggOver();
}