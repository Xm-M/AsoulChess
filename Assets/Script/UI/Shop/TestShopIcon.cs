using PixelsoftGames.PixelUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class TestShopIcon : MonoBehaviour
{
    public PropertyCreator good;
    public Image goodImage;
    public Image selfImage;
    //bool ifselect = false;
    public string team;
    public void InitShopIcon(PropertyCreator s,string team)
    {
        good = s;
        goodImage.sprite = good.chessSprite;
        this.team = team;
    }
    public void SelectCard()
    {
        PrePlantImage.instance.TryToPlant(good,CancelPlant,OnPlant,team);
    }
    //public override void OnPointerClick(PointerEventData eventData)
    //{
    //    if (!ifselect)
    //    {
    //        ifselect = true;
    //        goodImage.color = new Color(1, 1, 1, 0);
    //        //这里调用PrePlantImage的效果
    //        PrePlantImage.instance.TryToPlant(creator, CancelPlant, OnPlant);
    //        if (select != null) select.CancelPlant();
    //        select = this;
    //        au.RandomPlay();
    //    }
    //}
    public void CancelPlant()
    {
        //ifselect = false;
        //goodImage.color = new Color(1, 1, 1, 1);
        //if (select == this) select = null;
        Debug.Log("需要做什么吗");
    }
    public void OnPlant()
    {
        Debug.Log("种完了所以呢？");
    }
}
