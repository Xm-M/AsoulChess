using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
/// <summary>
/// 这个是上方的那个栏，懂我意思吧
/// 包括旁边的栏，反正这个就是整个选牌体系都放在这了
/// 我说实话，铲子不应该放在这个体系里
/// </summary>
public class PlantsShop : View
{
    //public static PlantsShop instance;
    public GameObject shopSelectIconPre;//这个是选牌的时候的那个栏
    public Transform selectIconParent;
    public GameObject shopIconPre;//这个是游戏开始的时候上面的那个牌
    public Transform shopIconParent;
    public PrePlantImage prePlant;
    List<ShopSelectIcon> allSelectIcons;
    List<ShopSelectIcon> currentSelectIcons;
    public Text sunLightText;
    //public ShovelPanel shovel;//不知道为什么这tm还有一个铲子的panel
    //public Chess selectChess;//也不是很知道selectChess的作用
    public int sunLight{get;private set;}
    public Animator anim;
    public AudioPlayer shopAudio;
    ShopIcon currentPlant;
    public override void Init()
    {
        //初始化要做什么呢？
        currentSelectIcons = new List<ShopSelectIcon>();
        allSelectIcons = new List<ShopSelectIcon>();
    }
    public override void Show()
    {
        base.Show();
        sunLight = 1000;
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
        if (currentSelectIcons .Count==0) return;
        for(int i = 0; i < allSelectIcons.Count; i++)
        {
            Destroy(allSelectIcons[i].gameObject);
        }
        for(int i = 0; i < currentSelectIcons.Count; i++)
        {
            Destroy(currentSelectIcons[i].gameObject);
        }
    }
    public void ChangeSunLight(int num){
        sunLight+=num;
        sunLightText.text=sunLight.ToString();
        EventController.Instance.TriggerEvent(EventName.WhenSunLightChange.ToString());
    }



    public void SelectPlant(ShopIcon icon)
    {
        if (currentPlant == null)
        {
            
            MapManage.instance.AwakeTile();
        }
        currentPlant = icon;
    }
    public bool IfCanBuyCard(PropertyCreator c){
        if(c.IfCanBuyCard(sunLight)){
            if (!prePlant.gameObject.activeSelf)
                EventController.Instance.AddListener<Tile>(EventName.WhenPlantChess.ToString(), BuyPlant);
            prePlant.transform.position= Input.mousePosition;
            prePlant.gameObject.SetActive(true);
            prePlant.image.sprite=c.chessSprite;
            
            return true;
        }
        return false;
    }
    public void BuyPlant(Tile t){
        //Debug.Log("b");
        if (currentPlant.good.IfCanPlant(t)) {
            //Debug.Log("a");
            Chess c = GameManage.instance.playerManage.CreateChess(currentPlant.good, t);
            ChangeSunLight(-c.propertyController.creator.baseProperty.price);
            prePlant.gameObject.SetActive(false);
            currentPlant.ColdDown();
            currentPlant = null;
            t.PlantChess(c);
            CancelBuyCard();
        }
    }
    public void CancelBuyCard(){
        MapManage.instance.SleepTile();
        prePlant.gameObject.SetActive(false);
        currentPlant=null;
        EventController.Instance.RemoveListener<Tile>(EventName.WhenPlantChess.ToString(), BuyPlant);
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

    public void GameStart(){
        GameManage.instance.GameStart();
        anim.Play("gameStart");
    }

    
}
