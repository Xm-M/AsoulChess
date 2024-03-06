using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemReward : Reward
{
    public ItemNames itemName;
    public Item item;
    private void Awake() {
        //item=GameManage.instance.itemManage.CreateItem(itemName);
    }
    public override void PickReward()
    {
        base.PickReward();
        UIManage.instance.ItemWindow.AddItem(item);
        Destroy(gameObject);
    }
}
