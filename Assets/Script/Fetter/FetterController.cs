using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 羁绊控制器 选完角色后进入游戏时，会给每个友军添加某种buff效果
/// </summary>
public class FetterController
{
    Dictionary<string, Fetter> fetterDic;
    public List<FetterDataList> fetterDataList;
    List<Fetter> lightFetter;

    public void InitController()
    {
        lightFetter = new List<Fetter>();
        fetterDic = new Dictionary<string, Fetter>();
        for (int i = 0; i < fetterDataList.Count; i++)
        {
            fetterDic.Add(fetterDataList[i].fetter.fetterName, fetterDataList[i].fetter);
        }
    }

    /// <summary>Tag 模式：统计 plantTags 包含 tag 的卡牌数</summary>
    static int CountByTag(List<PropertyCreator> roster, string tag)
    {
        int c = 0;
        foreach (var creator in roster)
        {
            if (creator.plantTags != null && creator.plantTags.Contains(tag))
                c++;
        }
        return c;
    }

    /// <summary>Member 模式：requiredMemberIds 中有多少唯一成员在场（同一成员多卡只算 1）</summary>
    static int CountByMember(List<PropertyCreator> roster, List<string> requiredMemberIds)
    {
        if (requiredMemberIds == null || requiredMemberIds.Count == 0) return 0;
        var present = new HashSet<string>();
        foreach (var creator in roster)
        {
            string mid = string.IsNullOrEmpty(creator.fetterMemberId) ? creator.chessName : creator.fetterMemberId;
            if (requiredMemberIds.Contains(mid))
                present.Add(mid);
        }
        return present.Count;
    }

    /// <summary>根据档位阈值返回当前档位，-1 表示未达标</summary>
    static int GetTier(int count, List<int> thresholds)
    {
        if (thresholds == null || thresholds.Count == 0) return -1;
        for (int i = thresholds.Count - 1; i >= 0; i--)
        {
            if (count >= thresholds[i]) return i;
        }
        return -1;
    }

    public void CheckFetter()
    {
        Debug.Log("检查羁绊人数中");
        UIManage.Show<FetterPanel>();

        var shop = UIManage.GetView<PlantsShop>();
        var roster = shop != null ? shop.GetFetterRosterCreators() : new List<PropertyCreator>();

        foreach (var kv in fetterDataList)
        {
            var fetter = kv.fetter;
            if (fetter == null) continue;
            var cfg = fetter.config ?? new FetterConfig();
            int count;
            int tier;
            if (fetter.detectMode == FetterDetectMode.Tag)
            {
                count = CountByTag(roster, cfg.tag);
                tier = GetTier(count, cfg.tierThresholds);
            }
            else
            {
                count = CountByMember(roster, cfg.requiredMemberIds);
                tier = (cfg.requiredMemberIds != null && cfg.requiredMemberIds.Count > 0 && count >= cfg.requiredMemberIds.Count) ? 0 : -1;
            }

            if (fetter.FetterLight(count, tier))
            {
                fetter.num = count;
                fetter.tier = tier;
                UIManage.GetView<FetterPanel>().ShowFetter(fetter);
                fetter.FetterEffect(count, tier);
                lightFetter.Add(fetter);
            }
        }
    }

    public Fetter GetFetter(string fetterName)
    {
        Fetter fetter = null;
        fetterDic?.TryGetValue(fetterName, out fetter);
        return fetter;
    }

    public void ClearFetter()
    {
        Debug.Log("清理羁绊");
        for (int i = 0; i < lightFetter.Count; i++)
        {
            lightFetter[i].ResetFetter();
        }
        lightFetter.Clear();
        UIManage.Close<FetterPanel>();
    }

    public bool ContainFetter(string fetterName)
    {
        foreach (var f in lightFetter)
        {
            if (f.fetterName == fetterName) return true;
        }
        return false;
    }
}
