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
        transform.position = Input.mousePosition;
        if (Input.GetMouseButtonDown(1))
        {
            UIManage.GetView<PlantsShop>().CancelBuyCard();
        }
        //if (Input.GetMouseButtonDown(1))
        //{
        //    if (!ifShovel)
        //        UIManage.GetView<PlantsShop>().CancelBuyCard();
        //    else
        //        UIManage.GetView<PlantsShop>().CancelShovel();
        //}
        //else if (ifShovel && Input.GetMouseButtonUp(0))
        //{
        //    UIManage.GetView<PlantsShop>().CancelShovel();
        //    if (UIManage.GetView<PlantsShop>().selectChess)
        //        UIManage.GetView<PlantsShop>().selectChess.RemoveChess();
        //}
    }


}
