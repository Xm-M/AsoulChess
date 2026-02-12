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
        // 检测是否点鼠标左键
        //Debug.Log("总不能是检测不到点击吧");
        if (!Input.GetMouseButtonDown(0))
            return false;
        //Debug.Log("点击了");
        if (cam == null) cam = Camera.main;


        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (clickBox.OverlapPoint(mouse))
        {
            return true;
        }
        return false;
    }
    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
         
    }
}