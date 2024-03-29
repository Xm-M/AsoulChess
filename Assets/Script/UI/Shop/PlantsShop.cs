using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
/// <summary>
/// ������Ϸ����Ǹ�����������˼��
/// �����Աߵ��������������������ѡ����ϵ����������
///  
/// </summary>
public class PlantsShop : View
{
    public GameObject shopSelectIconPre;//�����ѡ�Ƶ�ʱ����Ǹ���
    public Transform selectIconParent;
    public GameObject shopIconPre;//�������Ϸ��ʼ��ʱ��������Ǹ���
    public Transform shopIconParent;
    List<ShopSelectIcon> allSelectIcons;
    List<ShopSelectIcon> currentSelectIcons;
    
    public Animator anim;
    public AudioPlayer shopAudio;
    ShopIcon currentPlant;
    string Planttag = "Player";
    public override void Init()
    {
        //��ʼ��Ҫ��ʲô�أ�
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
    



    public void SelectPlant(ShopIcon icon)
    {
        //if (currentPlant == null)
        //{
            
        //    //MapManage.instance.AwakeTile();
        //}
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
        //prePlant.gameObject.SetActive(false);
        currentPlant.ColdDown();
        currentPlant = null;
        CancelBuyCard();
    }
    public void CancelBuyCard(){
        //MapManage.instance.SleepTile();
        //prePlant.gameObject.SetActive(false);
        currentPlant=null;
        //EventController.Instance.RemoveListener<Tile>(EventName.WhenPlantChess.ToString(), BuyPlant);
        //EventController.Instance.RemoveListener<Tile<(EventName.WhenPlantChess.ToString(),BuyPlant);
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
        //LevelManage.instance.GameStart();
        (MapManage_PVZ.instance as MapManage_PVZ ).WhenGameStart();
        anim.Play("gameStart");
    }

    
}
