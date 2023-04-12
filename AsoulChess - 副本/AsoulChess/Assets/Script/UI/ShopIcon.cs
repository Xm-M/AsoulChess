using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PixelsoftGames.PixelUI;
public class ShopIcon : MonoBehaviour
{
    public Chess good;
    public Image goodImage;

    public void AddNewGood(Chess chess){{
        //Debug.Log("add good");
        good=chess;
        goodImage.color=new Color(255,255,255,255);
        //goodImage.sprite=chess.property.chessSprite;
    }}
    public void BuyCard(){
        if(!good)return;
        Tile preTile=MapManage.instance.GetPreTile();
        if(preTile!=null){
            Chess c= ChessFactory.instance.ChessCreate(good,preTile,"Player");
            good=null;
            goodImage.color=new Color(0,0,0,0);
            EventController.Instance.TriggerEvent<Chess>(EventName.WhenBuyChess.ToString(),c);           
        }
        else Debug.Log("备战席已满");
    }
}
