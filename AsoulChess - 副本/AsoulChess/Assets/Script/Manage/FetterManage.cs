using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FetterManage
{   
    public string teamTag;
    public Dictionary<string, List<Fetter>> fetters;//每个羁绊类型，对应的羁绊们
    public Dictionary<string,int> fetterCounts;//每个羁绊的数量
    public Dictionary<string,int> chessNames;//可能上了多个棋子但羁绊只能触发一次
    //public List<Chess> team;

    public FetterManage(string tag)
    {
        fetters = new Dictionary<string, List<Fetter>>();
        fetterCounts=new Dictionary<string, int>();
        chessNames=new Dictionary<string, int>();
        teamTag=tag;
        //team = new List<Chess>();
        //EventController.Instance.AddListener(EventName.WhenSceneLoad.ToString(), Reset);
        EventController.Instance.AddListener<Chess>(EventName.ChessEnterDesk.ToString(),AddMember);
        EventController.Instance.AddListener<Chess>(EventName.ChessLeaveDesk.ToString(),RemoveMember);
        EventController.Instance.AddListener<string>(EventName.TeamDeath.ToString(),ResetAll);

        //EventController.Instance.AddListener(EventName.WhenSceneLoad.ToString(), Reset);
    }
    public void AddMember(Chess c)
    {
        
        if(!c.CompareTag(teamTag)||GameManage.instance.ifGameStart)return;
        Debug.Log(c.name+"进入战场");
        string chessName = c.name.Replace("(Clone)", "");
        int extra = 0;
        if (!chessNames.ContainsKey(chessName))
        {
           extra = 1; chessNames.Add(chessName,0);
        }
        chessNames[chessName]+=1;
        foreach (var fetter in c.fetters)
        {
           if (!fetters.ContainsKey(fetter.fetterName))
           {
               fetters.Add(fetter.fetterName, new List<Fetter>());
               fetterCounts.Add(fetter.fetterName, 0);
           }
           fetters[fetter.fetterName].Add(fetter);
           fetterCounts[fetter.fetterName] += extra;
           foreach (var f in fetters[fetter.fetterName])
           {
               f.FetterEffect(fetterCounts[fetter.fetterName]);
           }
        }
        EventController.Instance.TriggerEvent<FetterManage>(EventName.CheckFetter.ToString(),this);
        
    }
    public void RemoveMember(Chess c)
    {
        
        if(!c.CompareTag(teamTag)||GameManage.instance.ifGameStart)return;
        Debug.Log(c.name+"离开战场");
        string chessName = c.name.Replace("(Clone)", "");
        chessNames[chessName]--;
        
        if (chessNames[chessName] == 0)
        {
           chessNames.Remove(chessName);
           foreach (var fetter in c.fetters)
           {
               fetterCounts[fetter.fetterName]--;
               foreach(var f in fetters[fetter.fetterName])
               {
                   f.FetterEffect(fetterCounts[fetter.fetterName]);
               }
           }
        }
        foreach(var fetter in c.fetters){
            fetters[fetter.fetterName].Remove(fetter);
            fetter.FetterReSet();
        }
         EventController.Instance.TriggerEvent<FetterManage>(EventName.CheckFetter.ToString(),this);
        // team.Remove(c);
        // Debug.Log(c.name);
        // if (c.fetters != null)
        //    for (int i = 0; i < c.fetters.Count; i++)
        //    {
        //        //Debug.Log(c.name);
        //        fetters[c.fetters[i].fetterName].Remove(c.fetters[i]);
        //        for (int j = 0; j < fetters[c.fetters[i].fetterName].Count; j++)
        //        {
        //            fetters[c.fetters[i].fetterName][j].FetterReSet();
        //        }
        //    }
        // if (GameManage.instance.ifGameStart&& team.Count == 0)
        // {
        //     GameManage.instance.GameOver(c.tag);
        // }
    }
    public void ResetAll(string tag)
    {
        if(tag!=teamTag)return;
        fetters.Clear();
        fetterCounts.Clear();
        chessNames.Clear();
    }
}
