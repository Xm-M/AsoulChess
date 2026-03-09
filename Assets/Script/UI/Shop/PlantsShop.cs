using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using Sirenix.OdinInspector;
/// <summary>
/// 这个是上方的那个栏，懂我意思吧
/// 包括旁边的栏，反正这个就是整个选牌体系都放在这了
/// 问题是有些图是没有这个部署栏的怎么办
/// </summary>
public class PlantsShop : View
{
    public GameObject shopSelectIconPre;//植物选择卡牌
    public Transform selectIconParent;//这个是选牌的时候的那个栏
    public GameObject shopIconPre;//植物卡牌
    public Transform shopIconParent;//这个是游戏开始的时候上面的那个牌
    public List<ShopSelectIcon> currentSelectIcons;//
    public List<ShopIcon> currentShopIcons;//这个就是你进入游戏后有的牌了
    public List<ShopSelectIcon> allSelectIcons;//这个是你拥有的棋子 就是下面那个版的
    
    public int maxCount=10;
    public Animator anim;
    public AudioPlayer shopAudio;
    [FoldoutGroup("初始位置")]
    public Vector2 startPos1,startPos2;
    [FoldoutGroup("初始位置")]
    public RectTransform p1,p2;
    ShopIcon currentPlant;
    string Planttag = "Player";
    public override void Init()
    {
        //初始化要做什么呢？
        currentSelectIcons = new List<ShopSelectIcon>();
        allSelectIcons = new List<ShopSelectIcon>();
        currentShopIcons=new List<ShopIcon>();
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(),
            Hide);
    }
    public override void Show()
    {
        base.Show();
        currentSelectIcons.Clear();
        currentShopIcons.Clear();
        allSelectIcons.Clear();
        for (int i = 0; i < GameManage.instance.allChess.Count; i++)
        {
            ShopSelectIcon selectIcon = null;
            if(GameManage.instance.allChess[i].PlantEntrepotCardPre==null)
                selectIcon= Instantiate(shopSelectIconPre, selectIconParent).GetComponent<ShopSelectIcon>();
            else selectIcon = Instantiate(GameManage.instance.allChess[i].PlantEntrepotCardPre, selectIconParent).GetComponent<ShopSelectIcon>();
            selectIcon.InitSelectIcon(GameManage.instance.allChess[i]);
            allSelectIcons.Add(selectIcon);
        }
    }
    public override void Hide()
    {
        
        Debug.Log("PlantsShop清理完成");
        //if (currentSelectIcons .Count==0) return;
        for (int i = selectIconParent.childCount - 1; i >= 0; i--)
        {
            Destroy(selectIconParent.GetChild(i).gameObject);
        }
        allSelectIcons.Clear();
        //Debug.Log(selectIconParent.childCount);
        for (int i = shopIconParent.childCount - 1; i >= 0; i--)
        {
            Destroy(shopIconParent.GetChild(i).gameObject);
        }
        //Debug.Log(shopIconParent.childCount);
        currentSelectIcons.Clear();
        currentShopIcons.Clear();
        p1.anchoredPosition = startPos1;
        p2.anchoredPosition = startPos2;
        base.Hide();
    }
    public void SelectPlant(ShopIcon icon)
    {
        currentPlant = icon;
    }
    public bool IfCanBuyCard(PropertyCreator c){
        //Debug.Log("尝试购买");
        if(c.IfCanBuyCard()){
            PrePlantImage_Data data = new PrePlantImage_Data();
            data.creator = c;
            data.preSprite = c.chessSprite;
            data.tag = "Player";
            PrePlantImage.instance.TryToPlant( CancelBuyCard, BuyPlant,data,HandItemType.Plants);
            //Debug.Log("可以购买");
            return true;
        }
        return false;
    }
    public void BuyPlant(Chess chess){
        SunLightPanel.instance.ChangeSunLight(-currentPlant.good.baseProperty.price);
        currentPlant.ColdDown();
        currentPlant = null;
        CancelBuyCard();
    }
    public void CancelBuyCard(){
        currentPlant=null;
    }
    public bool AddSelection(ShopSelectIcon selectIcon){
        if(!currentSelectIcons.Contains(selectIcon)&&currentSelectIcons.Count<maxCount){
            currentSelectIcons.Add(selectIcon);
            GameObject shopIcon = null;
            if (selectIcon.select.PlantCardPre==null)
                shopIcon= Instantiate(shopIconPre,shopIconParent);
            else shopIcon= Instantiate(selectIcon.select.PlantCardPre, shopIconParent);
            shopIcon.GetComponent<ShopIcon>().InitShopIcon(selectIcon);
            return true;
        }return false;
    }
    public void RemoveSelection(ShopSelectIcon selectIcon){
        currentSelectIcons.Remove(selectIcon);
    }
    /// <summary>
    /// 这是铲掉植物
    /// </summary>
    public void DigPlant(Image image)
    {
        PrePlantImage_Data data = new PrePlantImage_Data();
        data.preSprite = image.sprite;
        PrePlantImage.instance.TryToPlant(() => shopAudio.PlaySub(0,"cancel"), (Chess) => shopAudio.PlaySub(0, "dig"), data,HandItemType.Shovel);
    }
    /// <summary>
    /// 这是使用锤子
    /// </summary>
    public void UseHammer(Image image)
    {
        PrePlantImage_Data data = new PrePlantImage_Data();
        data.preSprite = image.sprite;
        data.DM = new DamageMessege();
        data.DM.damage = 900;
        PrePlantImage.instance.TryToPlant(() => shopAudio.PlaySub(0, "cancel"), (Chess) => shopAudio.PlaySub(0, "dig"), data, HandItemType.Hammer);
    }

    /// <summary>
    /// 这个是button调用的
    /// </summary>
    public void GameStart(){
        (MapManage_PVZ.instance as MapManage_PVZ ).WhenGameStart();
        for(int i = 0; i < shopIconParent.childCount; i++)
        {
            shopIconParent.GetChild(i).GetComponent<ShopIcon>().SetClearColor();
        }
        anim.Play("gameStart");
        
    }
    public void Pause()
    {
        UIManage.GetView<ParsePanel>().ShowMenuPanel();
    }
    public void AddShopIcon(ShopIcon shopIcon)
    {
        currentShopIcons.Add(shopIcon);
    }
    public void RemoveShopIcon(ShopIcon shopicon)
    {
        currentShopIcons.Remove(shopicon);
    }
}
