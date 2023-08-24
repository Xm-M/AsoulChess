using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ShopSelectIcon : MonoBehaviour
{
    public PropertyCreator select;
    public Image selfImage;
    public Image chessImage;
    public Color selectColor;
    public Text price;

    public AudioSource Audio;
    //public int l;
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
            PlantsShop.instance.AddSelection(this);
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
