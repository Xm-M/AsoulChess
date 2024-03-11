using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ShovelPanel : MonoBehaviour
{
    public Image shovelImage;
    public Sprite shovelSprite;
    bool ifSelect;
    public void SelectImage()
    {
        if (!ifSelect&&GameManage.instance.ifGameStart)
        {
            PrePlantImage prePlantImage = UIManage.GetView<PlantsShop>().prePlant;
            UIManage.GetView<PlantsShop>().CancelBuyCard();
            shovelImage.gameObject.SetActive(false);
            prePlantImage.transform.position = Input.mousePosition;
            prePlantImage.gameObject.SetActive(true);
            //prePlantImage.image.sprite = shovelSprite;
            //prePlantImage.ifShovel = true;
            ifSelect = true;
        }
         
    }
    public void Cancle()
    {
        shovelImage.gameObject.SetActive(true);
        ifSelect = false;
        UIManage.GetView<PlantsShop>().prePlant.gameObject.SetActive(false);
        //PlantsShop.instance.prePlant.image.color = new Color(255, 255, 255, 125);
    }
}
