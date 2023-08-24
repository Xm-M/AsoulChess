using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChessMessegeUI : MonoBehaviour
{
    [SerializeField]Image chessImage;
    [SerializeField]Text[] texts;
    [SerializeField]Image[] weapons;
    Color zero,one;
    void Awake()
    {
        zero=new Color(0,0,0,0);
        one =new Color(255,255,255,255);
        chessImage.color=zero;
        foreach(var text in texts)
            text.text="0";
        foreach(var weapon in weapons){
            weapon.color=zero;
        }
        EventController.Instance.AddListener<Chess>(EventName.WhenSelectChess.ToString(),ShowMessege);
    }
    public void ShowMessege(Chess chess)
    {
        //chessImage.color=one;
        ////chessImage.sprite=chess.property.chessSprite;
        //int n=0;
        //foreach(var pro in chess.property.propertyDic){
        //    texts[n].text=pro.Value.currentValue.ToString();
        //    n++;
        //    if(n>=texts.Length)break;
        //}
        //foreach(var weapon in weapons){
        //    weapon.color=zero;
        //}
        //for(int i=0;i<chess.itemController.items.Count;i++){
        //    weapons[i].color=one;
        //    weapons[i].sprite=chess.itemController.items[i].ItemSprite;
        //}

    }
}
