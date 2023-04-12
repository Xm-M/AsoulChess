using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public enum FetterEnum
{
    Singer=1,
    Tank=2,
    Elf=3,
    Knights=4,
    Wild=5,
    Zombies=6,
}
[CreateAssetMenu(fileName = "NewFetterMessege", menuName = "FetterMessege")]
public class FetterMessege : ScriptableObject
{
    public List<FetterDate> fetters;
    public void ShowMessage(){
        foreach(var fetter in fetters){
            fetter.ShowMessege();
            Debug.Log(fetter.fetterType.ToString());

        }
    }
}
[Serializable]
public class FetterDate{
    public FetterEnum fetterType;
    public Sprite FetterIcon;

    public string chineseName;
    [SerializeReference]
    public Fetter fetter;

    public void ShowMessege()
    {
        if(fetter==null|| fetterType.ToString()!=fetter.fetterName){
            Type type = Type.GetType(fetterType.ToString());
            Debug.Log(type);
            fetter = Activator.CreateInstance(type) as Fetter;
        }
    }
}
