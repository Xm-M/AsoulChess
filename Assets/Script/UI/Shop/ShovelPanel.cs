using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 铲子面板 关于铲子 这个肯定也是要重新做的 因为可能一格内有多种植物
/// </summary>
public class ShovelPanel : MonoBehaviour
{
    public Image shovelImage;
    public Sprite shovelSprite;
    bool ifSelect;
    public void SelectImage()
    {
        if (!ifSelect&&GameManage.instance.ifGameStart)
        {
            PrePlantImage prePlantImage = PlantsShop.instance.prePlant;
            PlantsShop.instance.CancelBuyCard();
            shovelImage.gameObject.SetActive(false);
            prePlantImage.transform.position = Input.mousePosition;
            prePlantImage.gameObject.SetActive(true);
            prePlantImage.image.sprite = shovelSprite;
            prePlantImage.ifShovel = true;
            //prePlantImage.image.color = Color.white;
            ifSelect = true;
        }
         
    }
    public void Cancle()
    {
        shovelImage.gameObject.SetActive(true);
        ifSelect = false;
        PlantsShop.instance.prePlant.gameObject.SetActive(false);
        //PlantsShop.instance.prePlant.image.color = new Color(255, 255, 255, 125);
    }
}
