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
    public int price;
    float t;
    bool ifCanbuy;
    ShopSelectIcon selectIcon;
 
    public void AddNewGood(Chess chess){{
        
        //good=chess;
        goodImage.color=new Color(255,255,255,255);
         
    }}//
    public void InitShopIcon(ShopSelectIcon s){
        UIManage.GetView<PlantsShop>().AddShopIcon(this);
        this.selectIcon=s;
        good=selectIcon.select;
        goodImage.sprite=good.chessSprite;
        price = good.baseProperty.price;
        goodPrice.text=s.price.text;
        coldDown=good.baseProperty.CD;
        if (coldDown < 30) t = coldDown;
        else t = 0;
        ifCanbuy = true;
    }
    public void InitShopIcon(PropertyCreator creator)
    {
        good = creator;
        goodImage.sprite = good.chessSprite;
        price = good.baseProperty.price;
        goodPrice.text = creator.baseProperty.price.ToString();
        coldDown = good.baseProperty.CD;
        if (coldDown < 30) t = coldDown;
        else t = 0;
        ifCanbuy = true;
    }

    public void RefreshGood(PropertyCreator creator){

        good = creator;
        goodImage.sprite = good.chessSprite;
        price = creator.baseProperty.price;
        goodPrice.text = creator.baseProperty.price.ToString();
        coldDown=good.baseProperty.CD;
        if (coldDown < 30) t = coldDown;
        else t = 0;
        ifCanbuy=true;
    }
    public void ChangePrice(int changePrice)
    {
        price+=changePrice;
        goodPrice.text = price.ToString();
    }
    public void ResumePrice()
    {
        price = good.baseProperty.price;
        goodPrice.text = price.ToString();
    }

    public bool IfCanBuyCard(){
        if(good==null){
            Destroy(gameObject);
            return false;
        }
        bool ans=false;
        foreach(var tile in MapManage.instance.tiles)
        {
            ans=ans|good.IfCanPlant(tile);
        }
        return ans&& good.IfCanBuyCard()&& SunLightPanel.instance.sunLight>=price;
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
        var shop = UIManage.GetView<PlantsShop>();
        if (!LevelManage.instance.IfGameStart&&!shop.SelectOver)
        {
            selectIcon.UnselectCard();
            shop.shopAudio.PlayAudio("tap");
            shop.RemoveSelection(selectIcon);
            shop.RemoveShopIcon(this);
            Destroy(gameObject);
        }
        else
        {
            if (ifCanbuy && UIManage.GetView<PlantsShop>().IfCanBuyCard(good))
            {
                shop.SelectPlant(this);
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
