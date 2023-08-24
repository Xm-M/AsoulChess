using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FetterIcon : MonoBehaviour
{
    public Image fetterImage;
    public Text fetterName;
    public Text FetterCount;

    void Awake()
    {
         
    }
    public void ShowMessage(FetterDate fetter,int count){
        fetterImage.sprite=fetter.FetterIcon;
        fetterName.text=fetter.chineseName;
        FetterCount.text=count.ToString();
    }
}
