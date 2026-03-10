using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

/// <summary>
/// 被动技能和主动技能
/// </summary>

[Serializable]
public class SkillController:Controller
{
    [HideInInspector]public Chess user;
    [SerializeReference]
    public ISkill passiveSkill;//被动技能同理
    [SerializeReference]
    public ISkill activeSkill;//如果有复合技能 应该在Iskill类中制作一个复合技能而不是在这用list保存
    //[HideInInspector]
    public DamageMessege DM;
    public SkillContext context;
    [HideInInspector]public UnityEvent<Chess> onUseSkill;
    [HideInInspector]public UnityEvent<Chess> onSkillOver;
    /// <summary>当前技能释放中是否已触发 SkillEffect（动画某帧调用 UseSkill），读档时用于判断返还 CD 或正常结束</summary>
    [HideInInspector]public bool skillEffectFiredThisCast;
    public void InitController(Chess c){
        this.user=c;
        passiveSkill?.InitSkill(user);
        activeSkill?.InitSkill(user);
        //DM = new DamageMessege();
        context = new SkillContext();
    }
    public void WhenControllerEnterWar()
    {
        passiveSkill?.UseSkill(user);
        activeSkill?.WhenEnter(user);
        
    }
    public void WhenControllerLeaveWar()
    {
        //DM.damageType=DamageType.Magic;
        passiveSkill?.LeaveSkill(user);
        activeSkill?.LeaveSkill(user);
        onUseSkill.RemoveAllListeners();
        onSkillOver.RemoveAllListeners();
        context.Clear();
    }

    /// <summary>
    /// 这个其实是技能动画播放到释放技能的时候调用的
    /// 主要是播放Skill动画和实际技能使用是两回事 所以要分开讨论
    /// </summary>
    public void UseSkill()
    {
        skillEffectFiredThisCast = true;
        onUseSkill?.Invoke(user);
        activeSkill?.UseSkill(user);
    }
    /// <summary>
    /// 这个也是用在Transition用来判断是否转换的
    /// </summary>
    /// <returns></returns>
    public bool IfSkillReady()
    {
        if(activeSkill != null)
        {
            return activeSkill.IfSkillReady(user);
        }
        return false;
    }
    public void SkillOver(Chess user)
    {
        activeSkill.SkillOver(user);
        onSkillOver?.Invoke(user);
    }
}
public class SkillContext
{
    internal Dictionary<string, object> extra;
    public UnityEvent OnValueChange;
    public SkillContext()
    {
        extra = new Dictionary<string, object>();
        OnValueChange=new UnityEvent();
    }
    public void Clear()
    {
        extra.Clear();
        OnValueChange.RemoveAllListeners();
    }
    public void AddEvent(UnityAction unityAction)
    {
        Debug.Log("添加事件");
        OnValueChange.AddListener(unityAction);
    }
    public  void RemoveEvent(UnityAction unityAction)
    {
        OnValueChange.RemoveListener(unityAction);
    }
    public void Set<T>(string key, T value)
    {
        extra ??= new Dictionary<string, object>();
        extra[key] = value;
        OnValueChange?.Invoke();
        //Debug.Log("?");
    }
    public bool TryGet<T>(string key, out T value)
    {
        value = default;
        if (extra != null && extra.TryGetValue(key, out var obj) && obj is T cast)
        {
            value = cast;
            return true;
        }
        return false;
    }
    public void Remove(string key)
    {
        extra.Remove(key);
        OnValueChange?.Invoke();
    }

    static bool ShouldSkipKey(string key)
    {
        return key == "霸凌目标" || key == "stand" || key == "sword";
    }

    public SkillContextSaveData WriteToSaveData()
    {
        var data = new SkillContextSaveData();
        if (extra == null) return data;
        foreach (var kv in extra)
        {
            if (ShouldSkipKey(kv.Key)) continue;
            if (kv.Value is int i) data.Add(kv.Key, "int", i.ToString());
            else if (kv.Value is float f) data.Add(kv.Key, "float", f.ToString(System.Globalization.CultureInfo.InvariantCulture));
            else if (kv.Value is bool b) data.Add(kv.Key, "bool", b.ToString());
            else if (kv.Value is Chess c && c != null && c.moveController?.standTile != null)
            {
                var creator = c.propertyController?.creator?.chessName ?? "";
                data.Add(kv.Key, "chess", $"{creator}|{c.moveController.standTile.mapPos.x}|{c.moveController.standTile.mapPos.y}");
            }
        }
        return data;
    }

    public void RestoreFromSaveData(SkillContextSaveData data, Chess owner)
    {
        if (data?.keys == null) return;
        extra ??= new Dictionary<string, object>();
        for (int i = 0; i < data.keys.Count; i++)
        {
            var key = data.keys[i];
            var type = i < data.types?.Count ? data.types[i] : "";
            var val = i < data.values?.Count ? data.values[i] : "";
            if (type == "int" && int.TryParse(val, out int vi)) extra[key] = vi;
            else if (type == "float" && float.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float vf)) extra[key] = vf;
            else if (type == "bool" && bool.TryParse(val, out bool vb)) extra[key] = vb;
            else if (type == "chess") PendingChessRefs.Add((owner, key, val));
        }
    }

    /// <summary>待恢复的 Chess 引用：(owner, key, "creatorId|tileX|tileY")，读档后下一帧解析</summary>
    public static System.Collections.Generic.List<(Chess owner, string key, string chessData)> PendingChessRefs = new System.Collections.Generic.List<(Chess, string, string)>();
}
