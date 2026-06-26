using System;
using UnityEngine;

/// <summary>维什戴尔「残影」标记；绑定施加者，不可叠加（同名 BuffReset）。</summary>
[Serializable]
public class Buff_WisadelMark : Buff
{
    [NonSerialized] public Chess owner;

    public Buff_WisadelMark()
    {
        buffName = "WisadelMark";
    }

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
    }

    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        if (resetBuff is Buff_WisadelMark other)
            owner = other.owner;
    }

    public override Buff Clone()
    {
        var copy = new Buff_WisadelMark();
        copy.owner = owner;
        return copy;
    }

    public static bool HasMarkFrom(Chess target, Chess owner)
    {
        if (target?.buffController?.buffDic == null || owner == null)
            return false;
        if (!target.buffController.buffDic.TryGetValue("WisadelMark", out var buff))
            return false;
        if (buff is Buff_WisadelMark mark)
            return mark.owner == owner;
        return true;
    }

    public static void ApplyOrRefresh(Chess target, Chess owner)
    {
        if (target == null || owner == null)
            return;

        var mark = new Buff_WisadelMark { owner = owner };
        target.buffController.AddBuff(mark);
    }

    public static void ApplyOrRefresh(Chess target, Chess owner, Buff_WisadelMark template)
    {
        ApplyOrRefresh(target, owner);
    }

    public static void ClearAllFromOwner(Chess owner)
    {
        if (owner == null || GameManage.instance?.chessTeamManage == null)
            return;

        var enemies = GameManage.instance.chessTeamManage.GetEnemyTeam(owner.tag);
        if (enemies == null)
            return;

        for (int i = 0; i < enemies.Count; i++)
        {
            var e = enemies[i];
            if (e?.buffController == null)
                continue;
            if (e.buffController.buffDic.TryGetValue("WisadelMark", out var buff)
                && buff is Buff_WisadelMark mark
                && mark.owner == owner)
                buff.BuffOver();
        }
    }
}
