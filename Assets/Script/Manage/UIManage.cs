using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PixelsoftGames.PixelUI;
using UnityEngine.UI;
public class UIManage : MonoBehaviour
{
    public static UIManage instance;

    public BaseButton RoomButton;

    public Shop shop;

    public WeaponWindow ItemWindow;
    public UITabbedWindow gameOver;
    //public ChessMessage chessMessage;
    public GameObject SelectWindow;
    public Animator animator;
    public Text StadgeUI;
    public Text HpText;
    public GameObject damageText;
    public Dictionary<string, BaseUIComponent> UIComponentDic=new Dictionary<string, BaseUIComponent>();
    public GameObject baseText;
    public GameObject baseButton;
    public GameObject baseImage;
    public GameObject startGameUI;
    public GameObject waveZombieComming,LastWave;
    public AudioSource firstZombieComming;
    public UIStatBar zombieBar;
    public List<Image> flags;
  
    private void Awake()
    {
        if (instance == null||instance!=this) instance = this;
        EventController.Instance.AddListener(EventName.WaveZombieComming.ToString(), () => waveZombieComming.SetActive(true));
        EventController.Instance.AddListener(EventName.LastWaveZombie.ToString(), () => LastWave.SetActive(true));
        EventController.Instance.AddListener(EventName.FirstZombieComming.ToString(), () => firstZombieComming.Play());
        
        //DontDestroyOnLoad(gameObject);
    }
    public BaseImage CreateImage(string imageName,string parentName=null){
        GameObject image= ObjectPool.instance.Create(baseImage);
        image.transform.localScale=Vector3.one;
        if(parentName!=null)image.transform.SetParent(UIComponentDic[parentName].transform);
        BaseImage ans=image.GetComponent<BaseImage>();
        ans.UIName=imageName;
        if(UIComponentDic.ContainsKey(imageName))Debug.LogError("已经有这个ui");
        else UIComponentDic.Add(imageName,ans);
        return ans;
    }
    public BaseText CreateText(string textName,string parentName=null){
        GameObject text=ObjectPool.instance.Create(baseText);
        if(parentName!=null)text.transform.SetParent(UIComponentDic[parentName].transform);
        BaseText ans=text.GetComponent<BaseText>();
        if(UIComponentDic.ContainsKey(textName))Debug.LogError("已经有这个ui了");
        else UIComponentDic.Add(textName,ans);
        return ans;
    }
    public BaseButton CreateButton(string buttonName,string parentName=null){
        GameObject button =ObjectPool.instance.Create(baseButton);
        button.transform.localScale=Vector3.one;
        if(parentName!=null)button.transform.SetParent(UIComponentDic[parentName].transform);
        BaseButton ans=button.GetComponent<BaseButton>();
        if(UIComponentDic.ContainsKey(buttonName))Debug.LogError("已经有这个ui了");
        else UIComponentDic.Add(buttonName,ans);
        return ans;
    }
    public void DeleteBaseUIComponent(BaseUIComponent baseUIComponent){
        if(UIComponentDic.ContainsKey(baseUIComponent.UIName)){
            ObjectPool.instance.Recycle(UIComponentDic[baseUIComponent.UIName].gameObject);
            UIComponentDic.Remove(baseUIComponent.UIName);
        }
    }
    public void DeleteBaseUIComponent(string name){
        DeleteBaseUIComponent(UIComponentDic[name]);
    }

    public void RoomStart(){
        RoomManage.instance.RandomRoom();
    }
    public void CreateDamage(DamageMessege damageMessege)
    {
        GameObject d=ObjectPool.instance.Create(damageText);
        Text text = d.transform.GetChild(0).GetComponent<Text>();
        text.text = ((int)damageMessege.damage).ToString();
        d.transform.position = damageMessege.damageTo.transform.position;
    }
    
    public void Quit() => Application.Quit();
    public void CloseWindow(GameObject window) => window.SetActive(false);
    public void OpenWindow(GameObject window) => window.SetActive(true);

    public void AddUIComponent(BaseUIComponent baseUIComponent)
    {
        if(!UIComponentDic.ContainsKey(baseUIComponent.UIName))
            UIComponentDic.Add(baseUIComponent.UIName, baseUIComponent);
    }
    public void ShowChessMessage(Chess chess){
        
        //别的各种信息 
    }
    public void SetFlag(int n)
    {
        for (int i = 0; i < flags.Count ; i++)
        {
            flags[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < n; i++)
        {
            flags[i].gameObject.SetActive(true);
        }
    }
    
}
