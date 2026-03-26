using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>仅选中施法者自身（用于自疗子弹等）。</summary>
[Serializable]
public class FindTarget_Self : IFindTarget
{
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        if (user != null && !user.IfDeath)
            targets.Add(user);
    }
}
