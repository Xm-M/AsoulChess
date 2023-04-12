using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
[CreateAssetMenu(fileName ="AllItem",menuName ="Message/AllItem")]
public class AllItemMessage : ScriptableObject
{
    public ItemNames currentItem;
    public List<Item> AllItem;
    public Dictionary<ItemNames,Item> ItemDic;
    public void AddItem(){
        if(AllItem==null)AllItem=new List<Item>();
        if(ItemDic==null)ItemDic=new Dictionary<ItemNames, Item>();
        if(ItemDic.ContainsKey(currentItem))return;
        else{
            Item newItem=Activator.CreateInstance(Type.GetType(currentItem.ToString())) as Item;
            if(newItem!=null){
                AllItem.Add(newItem);
                ItemDic.Add(currentItem,newItem);
            }
        }
    }
}
