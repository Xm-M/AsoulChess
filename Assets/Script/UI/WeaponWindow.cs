using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponWindow : MonoBehaviour
{
   public List<WeaponIcon> ItemIcons;

   public void AddItem(Item item){
        foreach(var icon in ItemIcons){
            if(icon.AddItem(item))return;
        }
        Debug.Log("装备栏已经满");
   }
}
