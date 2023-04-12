using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FetterShowWindow : MonoBehaviour
{
    public Transform child;
    public GameObject fetteIconPre;
    public FetterMessege fetterDates;
    public Dictionary<string,FetterDate> FetterDateDic;
    public List<FetterIcon> icons;

    void Awake()
    {
        EventController.Instance.AddListener<FetterManage>(EventName.CheckFetter.ToString(),ShowFetterMessage);
        FetterDateDic=new Dictionary<string, FetterDate>();
        foreach(var fetter in fetterDates.fetters){
            FetterDateDic.Add(fetter.fetterType.ToString(),fetter);
        }
    }

    public void ShowFetterMessage(FetterManage fetterM){
        if(!CompareTag(fetterM.teamTag))return;
        int n=0;
        foreach(var fetter in fetterM.fetters){
            if(fetter.Value.Count>0){
                icons[n].gameObject.SetActive(true);
                icons[n].ShowMessage(FetterDateDic[fetter.Key],fetterM.fetterCounts[fetter.Key]);  
                n++;              
            }          
            if(n>icons.Count)break;
        }
        for(;n<icons.Count;n++)
            icons[n].gameObject.SetActive(false);
    }
}
