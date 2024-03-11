using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PixelsoftGames.PixelUI;

//shopincon肯定要改 这个是游戏中购买植物的那个卡牌懂吧
public class ShopIcon : MonoBehaviour
{
    public PropertyCreator good;
    public Image goodImage;
    public Image selfImage;
    public UIStatBar bar;
    public Color unUseCorlor;
    public Text goodPrice;
    public AudioSource Audio;

    public float coldDown;//冷却时间
    float t;
    ShopSelectIcon selectIcon;
    private void Awake() {
        EventController.Instance.AddListener(EventName.WhenSunLightChange.ToString(),IfCanBuyCard);
        EventController.Instance.AddListener(EventName.GameStart.ToString(),IfCanBuyCard);
    }
    private void OnDestroy() {
        EventController.Instance.RemoveListener(EventName.WhenSunLightChange.ToString(),IfCanBuyCard);
        EventController.Instance.RemoveListener(EventName.GameStart.ToString(),IfCanBuyCard);
    }
    public void AddNewGood(Chess chess){{
        
        //good=chess;
        goodImage.color=new Color(255,255,255,255);
         
    }}
    public void InitShopIcon(ShopSelectIcon s){
        this.selectIcon=s;
        good=selectIcon.select;
        goodImage.sprite=good.chessSprite;   
        goodPrice.text=s.price.text;
        coldDown=good.baseProperty.CD;
    }
    public void IfCanBuyCard(){
        if(good==null){
            Destroy(gameObject);
            return;
        }
        if(good.IfCanBuyCard(UIManage.GetView<PlantsShop>().sunLight)){
            selfImage.color=Color.white;
            goodImage.color=Color.white;
        }else{
            selfImage.color=unUseCorlor;
            goodImage.color=unUseCorlor;
        }
    }
    private void Update() {
        if(GameManage.instance.ifGameStart){
            this.t+=Time.deltaTime;
            bar.SetValue(coldDown-t, coldDown);
        }
    }
    
    public void BuyPlant(){
        if(t>coldDown&&GameManage.instance.ifGameStart&& UIManage.GetView<PlantsShop>().IfCanBuyCard(good)){
            UIManage.GetView<PlantsShop>().SelectPlant(this);
            Audio?.Play();
        }else if(!GameManage.instance.ifGameStart){
            selectIcon.UnselectCard();
            UIManage.GetView<PlantsShop>().RemoveSelection(selectIcon);
            Destroy(gameObject);
        }
    }
    public void ColdDown(){
        this.t=0;
    }
}
