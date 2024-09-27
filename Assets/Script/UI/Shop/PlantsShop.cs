using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using Sirenix.OdinInspector;
/// <summary>
/// 这个是上方的那个栏，懂我意思吧
/// 包括旁边的栏，反正这个就是整个选牌体系都放在这了
///  
/// </summary>
public class PlantsShop : View
{
    public GameObject shopSelectIconPre;//这个是选牌的时候的那个栏
    public Transform selectIconParent;
    public GameObject shopIconPre;//这个是游戏开始的时候上面的那个牌
    public Transform shopIconParent;
    List<ShopSelectIcon> allSelectIcons;
    List<ShopSelectIcon> currentSelectIcons;
    
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
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(),
            Hide);
        EventController.Instance.AddListener(EventName.WhenShovel.ToString(),
            CancelBuyCard);
    }
    public override void Show()
    {
        base.Show();
        currentSelectIcons.Clear();
        allSelectIcons.Clear();
        for (int i = 0; i < GameManage.instance.allChess.Count; i++)
        {
            ShopSelectIcon selectIcon = Instantiate(shopSelectIconPre, selectIconParent).GetComponent<ShopSelectIcon>();
            selectIcon.InitSelectIcon(GameManage.instance.allChess[i]);
            allSelectIcons.Add(selectIcon);
        }
    }
    public override void Hide()
    {
        base.Hide();
        Debug.Log("PlantsShop清理完成");
        if (currentSelectIcons .Count==0) return;
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
        p1.anchoredPosition = startPos1;
        p2.anchoredPosition = startPos2;
    }
    



    public void SelectPlant(ShopIcon icon)
    {
        currentPlant = icon;
    }
    public bool IfCanBuyCard(PropertyCreator c){
        if(c.IfCanBuyCard(SunLightPanel.instance.sunLight)){
            PrePlantImage.instance.TryToPlant(c, CancelBuyCard, BuyPlant);
            return true;
        }
        return false;
    }
    public void BuyPlant(){
        SunLightPanel.instance.ChangeSunLight(currentPlant.good.baseProperty.price);
        currentPlant.ColdDown();
        currentPlant = null;
        CancelBuyCard();
    }
    public void CancelBuyCard(){
        currentPlant=null;
    }
    public void AddSelection(ShopSelectIcon selectIcon){
        if(!currentSelectIcons.Contains(selectIcon)){
            currentSelectIcons.Add(selectIcon);
            GameObject shopIcon= Instantiate(shopIconPre,shopIconParent);
            shopIcon.GetComponent<ShopIcon>().InitShopIcon(selectIcon);
        }
    }
    public void RemoveSelection(ShopSelectIcon selectIcon){
        currentSelectIcons.Remove(selectIcon);
    }

    /// <summary>
    /// 这个是button调用的
    /// </summary>
    public void GameStart(){
        //(MapManage_PVZ.instance as MapManage_PVZ ).WhenGameStart();
        LevelManage.instance.GameStart();
        anim.Play("gameStart");
    }

    
}
