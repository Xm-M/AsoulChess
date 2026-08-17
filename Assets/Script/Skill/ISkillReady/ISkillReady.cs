using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface ISkillReady
{
    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets);
     public bool IfSkillReady(Chess user,SkillConfig config, List<Chess> targets);
}
public class SkillReady_IfTargetInRange : ISkillReady
{
    [SerializeReference]
    public IFindTarget search;
    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        search.FindTarget(user, targets);
        return targets.Count >= config.minTargetNum;
    }

    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        //throw new NotImplementedException();
    }
}
public class SkillReady_Multy : ISkillReady
{
    [SerializeReference]
    public List<ISkillReady> skillReadies;
    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        foreach (var skill in skillReadies)
        {
            if(!skill.IfSkillReady(user,config,targets))return false;
        }
        return true;
    }
    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        foreach(var skill in skillReadies)
        {
            skill.InitSkillReady(user,config,targets);
        }
    }
}
public class SkillReady_MouseDown : ISkillReady
{
    //public IFindTarget realFindTarget;   // 真正的索敌逻辑
    //public Box checkBox;                 // 点击检测区域（以 user 为中心）
    public BoxCollider2D clickBox;
    private Camera cam;

    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        // 暂停 / 未开战（GamePause 会置 IfGameStart=false）时点击不放技能
        if (LevelManage.instance == null || !LevelManage.instance.IfGameStart)
            return false;
        if (!Input.GetMouseButtonDown(0))
            return false;
        //Debug.Log("点击了");
        if (cam == null) cam = Camera.main;


        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (clickBox == null)
            return false;
        if (clickBox.OverlapPoint(mouse))
        {
            return true;
        }
        return false;
    }
    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        SkillColdFXPresenter.TrySpawnUnderChess(user);
    }
}
/// <summary>无刺有刺羁绊已触发且刺雨未在下的情况可用。用于主唱Nina主动技能。</summary>
public class SkillReady_RainNotActive : ISkillReady
{
    const string FetterName = "无刺有刺";
    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (!GameManage.instance.fetterManage.ContainFetter(FetterName)) return false;
        var fetter = GameManage.instance.fetterManage.GetFetter(FetterName) as TogenashiTogeari;
        return fetter != null && !fetter.IsRaining;
    }
    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets) { }
}
