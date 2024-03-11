using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="ShopRoom",menuName ="RoomType/ShopRoom")]
public class ShopRoom : RoomType
{
    public Chess business;
    public List<Chess> goods;
    Chess b;
    public override void WhenEnterRoom()
    {
        Debug.Log("ShopRoom");
        Tile t=MapManage.instance.tiles[7,4];
        //b=ChessFactory.instance.ChessCreate(business,t,"Mid");
        //UIManage.instance.shop.gameObject.SetActive(true);
        //UIManage.instance.shop.OpenShop(goods);
        //UIManage.instance.RoomButton.AddListener(LeaveRoom,"OverShopping");
    }
    public void LeaveRoom(){
        //ChessFactory.instance.ClearTeam("Mid");
        //UIManage.instance.shop.gameObject.SetActive(false);  
        //UIManage.instance.RoomButton.ClearButton();
        RoomManage.instance.RandomRoom();  
    }

    public override void WhenLeaveRoom()
    { 
        base.WhenLeaveRoom();
    }
}
