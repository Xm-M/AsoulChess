using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public class BaseButton : BaseUIComponent
{
    public Button baseButton;
    public Image image;
    public Text label;
    public void AddListener(UnityAction action){
        baseButton.onClick.AddListener(action);
    }
    public void AddListener(UnityAction action,string label){
        baseButton.onClick.AddListener(action);
        if(label!=null)this.label.text=label;
    }
    public void BanButton(){
        baseButton.interactable=false;
    }
    public void ReUse(){
        baseButton.interactable=true;
    }
    public void ClearButton(){
        baseButton.onClick.RemoveAllListeners();
    }
}
