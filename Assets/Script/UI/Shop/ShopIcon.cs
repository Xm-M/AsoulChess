using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PixelsoftGames.PixelUI;

/// <summary>
/// shopincon肯定要改 这个是游戏中购买植物的那个卡牌懂吧 
/// 就是上面那个栏里的
/// 思考一下这里到底哪里有问题
/// 主要是需要判断的只要一个就行
/// 把变色放在一个函数 然后判断函数一个个判断就好了
/// 基本的判断有 1.是否买得起 2.冷却是否结束 3.再来一个PropertyCreator的函数里的判断函数
/// </summary>
public class ShopIcon : MonoBehaviour
{
    public PropertyCreator good;
    public Image goodImage;
    public Image selfImage;
    public UIStatBar bar;
    public Color unUseCorlor;
    public Text goodPrice;
    public AudioPlayer Audio;
    public float coldDown;//冷却时间
    float t;
    bool ifCanbuy;
    ShopSelectIcon selectIcon;
 
    public void AddNewGood(Chess chess){{
        
        //good=chess;
        goodImage.color=new Color(255,255,255,255);
         
    }}//
    public void InitShopIcon(ShopSelectIcon s){
        this.selectIcon=s;
        good=selectIcon.select;
        goodImage.sprite=good.chessSprite;   
        goodPrice.text=s.price.text;
        coldDown=good.baseProperty.CD;
        if (coldDown < 30) t = coldDown;
        else t = 0;
        ifCanbuy = true;
    }
    public bool IfCanBuyCard(){
        if(good==null){
            Destroy(gameObject);
            return false;
        }
      
        return good.IfCanBuyCard(SunLightPanel.instance.sunLight);
    }
    public bool IfColdDown()
    {
        if (t <= coldDown)
        {
            this.t += Time.deltaTime;
            bar.SetValue(coldDown - t, coldDown);
            return false;
        }
        else
        {
            return true;
        }
        //return (t > coldDown);
        
    }
    /// <summary>
    /// 所以说本来就有update 
    /// </summary>
    private void Update() {
        if(LevelManage.instance.IfGameStart)
        {
            bool buy= IfCanBuyCard();
            bool cd= IfColdDown();
            if (buy && cd&&!ifCanbuy)
            {
                SetWhiteColor();
            }
            else if((!buy||!cd)&&ifCanbuy)
            {
                SetClearColor();
            }
        }
    }
    
    /// <summary>
    /// 这个肯定是绑定在Button的OnClick函数上的啊 还要想吗
    /// </summary>
    public void BuyPlant(){
        if (!LevelManage.instance.IfGameStart)
        {
            selectIcon.UnselectCard();
            UIManage.GetView<PlantsShop>().shopAudio.PlayAudio("tap");
            UIManage.GetView<PlantsShop>().RemoveSelection(selectIcon);
            Destroy(gameObject);
        }
        else
        {
            if (ifCanbuy && UIManage.GetView<PlantsShop>().IfCanBuyCard(good))
            {
                UIManage.GetView<PlantsShop>().SelectPlant(this);
                Audio?.PlayAudio("Plant");
            }
            else 
            {
                Audio?.PlayAudio("CantPlant");
            }
        }
    } 
    public void SetWhiteColor()
    {
        selfImage.color = Color.white;
        goodImage.color = Color.white;
        ifCanbuy = true;
    }
    public void SetClearColor()
    {
        selfImage.color = unUseCorlor;
        goodImage.color = unUseCorlor;
        ifCanbuy = false;
    }
    public void ColdDown(){
        this.t=0;
    }
}
