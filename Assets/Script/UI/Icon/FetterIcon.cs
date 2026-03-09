using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// 单纯的用来显示羁绊的图标
/// </summary>
public class FetterIcon : MonoBehaviour
{
    public Image fetterImage;
    public TMP_Text fetterName;
    public Text fetterCount;
    //public Text fetterCount;
    public void ShowFetterIcon(Fetter fetter)
    {
        fetterImage.sprite=fetter.fetterIcon;
        fetterName.text=fetter.fetterName;
        fetterCount.text = fetter.num.ToString();
        //fetterCount.text=fetter.
    }
}
