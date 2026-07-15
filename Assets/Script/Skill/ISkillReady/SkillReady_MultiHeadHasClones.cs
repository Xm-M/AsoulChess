using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>多首主动技就绪：≥1 头、未终局锁；点击自身碰撞体释放。</summary>
[Serializable]
public class SkillReady_MultiHeadHasClones : ISkillReady
{
    [Tooltip("为 true 时需点击棋子碰撞体；false 则 CD 满即可（调试用）")]
    public bool requireClick = true;

    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || MultiHeadMutsumiKeys.IsFinaleLocked(user))
            return false;
        if (MultiHeadMutsumiKeys.GetCloneCount(user) <= 0)
            return false;
        if (!requireClick)
            return true;
        if (!Input.GetMouseButtonDown(0))
            return false;
        Camera cam = Camera.main;
        if (cam == null)
            return false;
        Collider2D col = user.GetComponent<Collider2D>();
        if (col == null)
            return false;
        Vector2 mouse = cam.ScreenToWorldPoint(Input.mousePosition);
        return col.OverlapPoint(mouse);
    }

    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        SkillColdFXPresenter.TrySpawnUnderChess(user);
    }
}
