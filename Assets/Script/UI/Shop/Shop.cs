using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public static Shop instance;
    public ShopIcon[] shopList;
    public int ShopLevel;
    public List<Chess> chessList;
    private void Awake()
    {
        if (instance == null) instance = this;
            EventController.Instance.AddListener(EventName.RestartGame.ToString(), ResetAll);
            ResetAll();
    }
    public void OpenShop(List<Chess> chesses){
        chessList=chesses;
        RefreshIcon();
    }
    public void CloseShop(){
        gameObject.SetActive(false);
    }

    public void ResetAll()
    {
        Debug.Log(chessList.Count);
        foreach (var icon in shopList)
        {            
            icon.AddNewGood(chessList[Random.Range(0,chessList.Count)]);
        }
    }
    public void RefreshIcon(){
        foreach (var icon in shopList)
        {            
            icon.AddNewGood(chessList[Random.Range(0,chessList.Count)]);
        }
    }
    public void ShopLevelUp(){

    }
    public virtual void AddSelection(ShopSelectIcon select,int level)
    {

    }
    public virtual void RemoveSelect(){
        
    }
    public void Sell()
    {
        // if (select != null)
        // {
        //     for (int i = 0; i < shopList.Length; i++)
        //     {
        //         if (shopList[i].ShopSelectIcon == select)
        //         {
        //             shopList[i].ResetAll();
        //             shopList[i].ShowGood();
        //             num--;
        //         }
        //     }
        //     CharacterWindow.instance.iconList.Remove(select.gameObject);
        //     Destroy(select.gameObject);
        //     ShopWindow.instance.SellChess();
        
    }
    
}
