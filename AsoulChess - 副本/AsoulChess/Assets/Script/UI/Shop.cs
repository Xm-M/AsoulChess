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
    
}
