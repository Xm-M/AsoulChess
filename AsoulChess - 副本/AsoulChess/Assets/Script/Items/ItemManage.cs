using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
public class ItemManage : MonoBehaviour
{
    //可以用读取文件的方式访问其实，但是我不
    [SerializeField]List<ItemNameSpriteParis> AllItemSprite;
    List<ItemGameObjectParis> playerHoldItem;
    Dictionary<ItemNames,Sprite> itemDic;

    int itemCount;
    void Awake()
    {
        itemDic=new Dictionary<ItemNames, Sprite>();
        playerHoldItem=new List<ItemGameObjectParis>();
        foreach(var itemparis in AllItemSprite){
            itemDic.Add(itemparis.itemName,itemparis.itemSprite);
        }
        
    }
    public Item CreateItem(ItemNames itemName){
        Item ans=Activator.CreateInstance(Type.GetType(itemName.ToString())) as Item;
        ans.instanceID=itemCount;
        itemCount++;
        ans.ItemName=itemName;
        ans.ItemSprite=itemDic[itemName];
        return ans;
    }
    public void AddNewPlayerItem(ItemNames item){
        Item newItem=CreateItem(item);
        AddNewPlayerItem(newItem);
    }
    public void AddNewPlayerItem(Item item){
        ItemGameObjectParis newItem=new ItemGameObjectParis();
        newItem.item=item;
        BaseButton itemButton=UIManage.instance.CreateButton("PlayerItem"+item.instanceID,"PlayerWeaponList");
        Debug.Log(itemButton);
        itemButton.image.sprite=item.ItemSprite;
        itemButton.AddListener(delegate{
            EquipItem(item);
        });
        itemButton.transform.localScale=Vector3.one;
        playerHoldItem.Add(newItem);
    }
    public void EquipItem(Item item){
        Debug.Log("EquipWeapon ");
        Chess hand=GameManage.instance.HandChess;
        Debug.Log(hand);
        if(hand&&hand.CompareTag("Player")){
            Debug.Log("?");
            hand.itemController.AddItem(item);
            UIManage.instance.DeleteBaseUIComponent("PlayerItem"+item.instanceID);
        }
    }
}
[Serializable]
public class ItemNameSpriteParis{
    public ItemNames itemName;
    public Sprite itemSprite;
    public Item item;
}
public class ItemGameObjectParis{
    public Item item;
    public GameObject itemObject;
}
