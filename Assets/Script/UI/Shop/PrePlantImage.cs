using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PrePlantImage : MonoBehaviour
{
    public Image image;
    public bool ifShovel;
    void Update()
    {
        transform.position= Input.mousePosition;
        if(Input.GetMouseButtonDown(1)){
            if (!ifShovel)
                PlantsShop.instance.CancelBuyCard();
            else
                PlantsShop.instance.CancelShovel();
        }else if (ifShovel&&Input.GetMouseButtonUp(0))
        {
            PlantsShop.instance.CancelShovel();
            if(PlantsShop.instance.selectChess)
                PlantsShop.instance.selectChess.RemoveChess();
        }
    }
     
     
}
