using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
public class ShopSelectIcon : MonoBehaviour
{
    public PropertyCreator select;
    public Image selfImage;
    public Image chessImage;
    public Color selectColor;
    public Text price;

    public AudioPlayer Audio;
    public bool ifSelect;
    private void OnEnable()
    {
        ifSelect = false;
    }
    public void InitSelectIcon(PropertyCreator c){
        this.select=c;
        price.text=c.baseProperty.price.ToString();
        chessImage.sprite = c.chessSprite;
    }
    public void SelectCard()
    {
        if (select != null && !ifSelect)
        {
            UIManage.GetView<PlantsShop>().AddSelection(this);
            selfImage.color=selectColor;
            chessImage.color=selectColor;
            ifSelect=true;
            Audio?.Play();
        }
    }
    public void UnselectCard(){
        ifSelect=false;
        selfImage.color=Color.white;
        chessImage.color=Color.white;
    }
}
