using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 羁绊控制器 选完角色后进入游戏时，会给每个友军添加某种buff效果
/// 注册GameStart事件 然后
/// 
/// </summary>
public class FetterController
{
    Dictionary<string, int> fetterNumDic;
    Dictionary<string, Fetter> fetterDic;
    public List<FetterDataList> fetterDataList;
    List<Fetter> lightFetter;
    public void InitController()
    {
        lightFetter = new List<Fetter>();
        fetterNumDic = new Dictionary<string, int>();
        fetterDic = new Dictionary<string, Fetter>();
        for(int i = 0; i < fetterDataList.Count; i++)
        {
            fetterDic.Add(fetterDataList[i].fetter.fetterName, fetterDataList[i].fetter);
        }

        //EventController.Instance.AddListener(EventName.GameStart.ToString(),CheckFetter);
        //EventController.Instance.AddListener(EventName.GameOver.ToString(), ClearFetter);
        //EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), ClearFetter);
    }

    /// <summary>
    /// 要检测的是PlantShop里currentSelect里的所有棋子信息 
    /// 然后将获取他们tag 然后统计数量 查看是否触发羁绊
    /// 步骤就是先统计selecticons里各个羁绊的数量是多少 然后分别统计他们是否满足羁绊效果 如果满足就触发羁绊效果
    /// </summary>
    public void CheckFetter()
    {
        Debug.Log("检查羁绊人数中");
        UIManage.Show<FetterPanel>();
        //统计各羁绊的人数
        for (int i = 0; i < UIManage.GetView<PlantsShop>().currentShopIcons.Count; i++)
        {
            List<string> tags =
            UIManage.GetView<PlantsShop>().currentShopIcons[i].good.plantTags;
            foreach(var tag in tags)
            {
                if (fetterNumDic.ContainsKey(tag)) fetterNumDic[tag]++;
                else fetterNumDic.Add(tag, 1);
            }   
        }
        //根据统计结束的数量决定是否要生成对应的fetter
        foreach(var f in fetterNumDic)
        {
            if (fetterDic.ContainsKey(f.Key))
            {
                //说明点亮了
                if (fetterDic[f.Key].FetterLight(f.Value))
                {
                    UIManage.GetView<FetterPanel>().ShowFetter(fetterDic[f.Key]);
                    fetterDic[f.Key].FetterEffect(f.Value);
                    lightFetter.Add(fetterDic[f.Key]);
                }
            }
        }
    }
    public Fetter GetFetter(string fetterName)
    {
        Fetter fetter = null;   
        fetterDic.TryGetValue(fetterName, out fetter);
        return fetter;
    }
    public void ClearFetter()
    {
        Debug.Log("清理羁绊");
        for(int i = 0; i < lightFetter.Count; i++)
        {
            lightFetter[i].ResetFetter();
        }
        fetterNumDic.Clear();
        //fetterDic.Clear();
        lightFetter.Clear();
        UIManage.Close<FetterPanel>();

    }
    public bool ContainFetter(string fetterName)
    {
        foreach(var f in lightFetter)
        {
            if (f.fetterName == fetterName)
            {
                return true;
            }
        }
        return false;
    }
} 
