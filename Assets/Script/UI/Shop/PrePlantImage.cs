using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PrePlantImage : MonoBehaviour
{
    public Image image;
    public bool ifShovel;//这个是铲子的意思
    public LayerMask ground;
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
        }else if (Input.GetMouseButtonDown(0))
        {
            Check();
        }
    }
    public void Check()
    {
        Collider2D collider= Physics2D.OverlapCircle(Camera.main.ScreenToWorldPoint(Input.mousePosition)
            , 0.2f, ground);
        Debug.Log(transform.position);
        Debug.Log(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        Debug.Log(collider);
        if(collider != null)
        {
            Debug.Log(collider);
            Tile tile = collider.gameObject.GetComponent<Tile>();
            if (PlantsShop.instance.currentPlant.good.IfCanPlant(tile))
            {
                PlantsShop.instance.BuyPlant(tile);
            }
        }
    }
     
}
