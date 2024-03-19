using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
/// <summary>
/// ������Ϸ����Ǹ�����������˼��
/// �����Աߵ��������������������ѡ����ϵ����������
/// ��˵ʵ�������Ӳ�Ӧ�÷��������ϵ��
/// </summary>
public class PlantsShop : View
{
    //public static PlantsShop instance;
    public GameObject shopSelectIconPre;//�����ѡ�Ƶ�ʱ����Ǹ���
    public Transform selectIconParent;
    public GameObject shopIconPre;//�������Ϸ��ʼ��ʱ��������Ǹ���
    public Transform shopIconParent;
    public PrePlantImage prePlant;
    List<ShopSelectIcon> allSelectIcons;
    List<ShopSelectIcon> currentSelectIcons;
    public Text sunLightText;
    //public ShovelPanel shovel;//��֪��Ϊʲô��tm����һ�����ӵ�panel
    //public Chess selectChess;//Ҳ���Ǻ�֪��selectChess������
    public int sunLight{get;private set;}
    public Animator anim;
    public AudioPlayer shopAudio;
    ShopIcon currentPlant;
    public override void Init()
    {
        //��ʼ��Ҫ��ʲô�أ�
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
