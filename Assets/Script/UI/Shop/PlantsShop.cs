using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
public class PlantsShop : MonoBehaviour
{
    public static PlantsShop instance;
    public GameObject shopSelectIconPre;
    public Transform selectIconParent;
    public GameObject shopIconPre;
    public Transform shopIconParent;

    public PrePlantImage prePlant;
    public PlayableDirector director;
    List<ShopSelectIcon> allSelectIcons;
    List<ShopSelectIcon> currentSelectIcons;
    public Text sunLightText;
    public ShovelPanel shovel;
    public Chess selectChess;
    public int sunLight{get;private set;}

    ShopIcon currentPlant;

    void Awake()
    {
        if(instance==null){
            instance=this;
            FirstOpenShop();
            //EventController.Instance.AddListener(EventName.GameStart.ToString(),()=>director.Resume());
        }else{
            Destroy(gameObject);
        }
    }
    void FirstOpenShop(){
        sunLight=1000;
        currentSelectIcons=new List<ShopSelectIcon>();
        allSelectIcons=new List<ShopSelectIcon>();
        for(int i=0;i<GameManage.instance.allChess.Count;i++){
            ShopSelectIcon selectIcon=Instantiate(shopSelectIconPre,selectIconParent).GetComponent<ShopSelectIcon>();
            selectIcon.InitSelectIcon(GameManage.instance.allChess[i]);
            allSelectIcons.Add(selectIcon);
        }
    }
    public void ChangeSunLight(int num){
        sunLight+=num;
        sunLightText.text=sunLight.ToString();
        EventController.Instance.TriggerEvent(EventName.WhenSunLightChange.ToString());
    }
    public bool IfCanBuyCard(PropertyCreator c){
        //Debug.Log(c);
        //int price=c.baseProperty.price;
        if(c.IfCanBuyCard(sunLight)){
            shovel.Cancle();
            prePlant.transform.position= Input.mousePosition;
            prePlant.gameObject.SetActive(true);
            prePlant.image.sprite=c.chessSprite;
            prePlant.ifShovel = false;
            
            return true;
        }
        return false;
    }
    public void SelectPlant(ShopIcon icon){
        if(currentPlant==null){
            currentPlant=icon;
            MapManage.instance.AwakeTile();
        }
    }
    public void BuyPlant(Tile t){
        Chess c=GameManage.instance.playerManage.CreateChess(currentPlant.good,t);
        c.WhenChessEnterWar();
        EventController.Instance.TriggerEvent<Chess>(EventName.ChessEnterDesk.ToString(),c);
        ChangeSunLight(-c.propertyController.creator.baseProperty.price);
        prePlant.gameObject.SetActive(false);
        currentPlant.ColdDown();
        currentPlant=null;
    }
    public void CancelBuyCard(){
        MapManage.instance.SleepTile();
        prePlant.gameObject.SetActive(false);
        currentPlant=null;
    }
    public void CancelShovel()
    {
        shovel.Cancle();
    }
    public void AddSelection(ShopSelectIcon selectIcon){
        Debug.Log(currentSelectIcons.Count);
        if(!currentSelectIcons.Contains(selectIcon)){
            Debug.Log("?");
            currentSelectIcons.Add(selectIcon);
            GameObject shopIcon= Instantiate(shopIconPre,shopIconParent);
            shopIcon.GetComponent<ShopIcon>().InitShopIcon(selectIcon);
        }
    }
    public void RemoveSelection(ShopSelectIcon selectIcon){
        currentSelectIcons.Remove(selectIcon);
    }

    public void GameStart(){
        director.Resume();
        GameManage.instance.GameStart();
    }
}
