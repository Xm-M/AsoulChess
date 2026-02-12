using PixelsoftGames.PixelUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 这个b类是用来干嘛的
/// </summary>

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
        PrePlantImage_Data data = new PrePlantImage_Data();
        data.creator = good;
        data.preSprite = good.chessSprite;
        data.tag = team;
        PrePlantImage.instance.TryToPlant( CancelPlant,OnPlant,data,HandItemType.Plants);
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
    public void OnPlant(Chess chess)
    {
        Debug.Log("种完了所以呢？");
    }
}
