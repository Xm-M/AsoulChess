using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 这个是在仓库里的选项牌 
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

    [Tooltip("肉鸽 HUD 卡组只读预览：仅展示详情，不加入出战栏")]
    public bool viewOnly;

    private void OnEnable()
    {
        ifSelect = false;
    }
    public void InitSelectIcon(PropertyCreator c){
        this.select=c;
        if (price != null)
            price.text = viewOnly ? "" : c.baseProperty.price.ToString();
        chessImage.sprite = c.chessSprite;
    }
    public void SelectCard()
    {
        if (viewOnly)
        {
            if (select != null)
                UIManage.GetView<RoguelikeRunInfoPanel>()?.ShowPlantDetail(select);
            return;
        }

        if (select != null)
            UIManage.GetView<PlantsShop>().ShowPlantDetail(select);

        if (select != null && !ifSelect)
        {
            if (UIManage.GetView<PlantsShop>().AddSelection(this))
            {
                selfImage.color = selectColor;
                chessImage.color = selectColor;
                ifSelect = true;
                Audio?.Play();
            }
        }
    }
    public void UnselectCard(){
        ifSelect=false;
        selfImage.color=Color.white;
        chessImage.color=Color.white;
    }
}
