using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitSelectShopPanel : View
{
    public Transform iconParent;
    List<PropertyCreator> creators;
    public GameObject shopIconPre;//植物卡牌
    public override void Init()
    {
        EventController.Instance.AddListener(EventName.GameOver.ToString(), Hide);
        creators = new List<PropertyCreator>();
    }
    public void InitCreator(List<PropertyCreator> cs)
    {
        creators.AddRange(cs);
        foreach(var creator in creators)
        {
            GameObject shopIcon = null;
            if (creator.PlantCardPre == null)
                shopIcon = Instantiate(shopIconPre, iconParent);
            else shopIcon = Instantiate(shopIconPre, iconParent);
            shopIcon.GetComponent<ShopIcon>().InitShopIcon(creator);
        }
    }
    public override void Hide()
    {
        creators.Clear();
        base.Hide();
    }
    public override void Show()
    {
        base.Show();
    }
}
