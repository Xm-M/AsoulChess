using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using Sirenix.OdinInspector;
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
    [FoldoutGroup("��ʼλ��")]
    public Vector2 startPos1,startPos2;
    [FoldoutGroup("��ʼλ��")]
    public RectTransform p1,p2;
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
        Debug.Log("PlantsShop�������");
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
    /// �����button���õ�
    /// </summary>
    public void GameStart(){
        //(MapManage_PVZ.instance as MapManage_PVZ ).WhenGameStart();
        LevelManage.instance.GameStart();
        anim.Play("gameStart");
    }

    
}
