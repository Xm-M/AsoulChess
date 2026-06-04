using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 本局生效道具管理（对标 <see cref="FetterController"/>）。
/// 来源：<see cref="GameManage.playerOwnedProps"/>（由 <c>ownedPropIds</c> 解析），为空时回退 <see cref="GameManage.allProps"/>。
/// </summary>
public class PropController
{
    Dictionary<string, PropItemData> propDic;
    readonly List<PropItemData> activeProps = new List<PropItemData>();
    PropContext context;

    public IReadOnlyList<PropItemData> ActiveProps => activeProps;

    public void InitController()
    {
        activeProps.Clear();
        propDic = new Dictionary<string, PropItemData>();
        if (GameManage.instance?.allProps == null) return;
        for (int i = 0; i < GameManage.instance.allProps.Count; i++)
        {
            var p = GameManage.instance.allProps[i];
            if (p == null) continue;
            string id = p.GetPropId();
            if (!propDic.ContainsKey(id))
                propDic.Add(id, p);
        }
    }

    public PropItemData GetProp(string propId)
    {
        if (string.IsNullOrEmpty(propId) || propDic == null) return null;
        propDic.TryGetValue(propId, out var p);
        return p;
    }

    public void CheckProps()
    {
        activeProps.Clear();
        var sources = ResolvePropSources();
        if (sources != null)
        {
            for (int i = 0; i < sources.Count; i++)
            {
                var p = sources[i];
                if (p != null && p.effect != null)
                    activeProps.Add(p);
            }
        }

        context = new PropContext { controller = this };
        for (int i = 0; i < activeProps.Count; i++)
            activeProps[i].effect.Apply(context);

        var panel = UIManage.GetView<PropPanel>();
        panel?.Refresh(activeProps);
    }

    public void ClearProps()
    {
        if (context != null)
        {
            for (int i = 0; i < activeProps.Count; i++)
            {
                if (activeProps[i]?.effect != null)
                    activeProps[i].effect.Remove(context);
            }
        }

        activeProps.Clear();
        context = null;
        UIManage.GetView<PropPanel>()?.ClearIcons();
    }

    static List<PropItemData> ResolvePropSources()
    {
        if (GameManage.instance == null) return null;
        var owned = GameManage.instance.playerOwnedProps;
        if (owned != null && owned.Count > 0)
            return owned;
        return GameManage.instance.allProps;
    }
}
