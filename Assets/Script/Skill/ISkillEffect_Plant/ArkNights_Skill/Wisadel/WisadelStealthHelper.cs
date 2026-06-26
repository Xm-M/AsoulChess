using System.Collections.Generic;
using UnityEngine;

/// <summary>维什戴尔隐匿：四邻格存在存活己方魂灵之影时进入无法选中状态。</summary>
public static class WisadelStealthHelper
{
    const string CamouflageBuffName = "WisadelCamouflage";

    public static void Refresh(Chess master)
    {
        if (master == null || master.IfDeath || master.buffController == null)
            return;

        bool shouldHide = HasAdjacentOwnedShadow(master);
        bool hasBuff = master.buffController.buffDic.TryGetValue(CamouflageBuffName, out Buff buff);

        if (shouldHide && !hasBuff)
            master.buffController.AddBuff(new Buff_WisadelCamouflage());
        else if (!shouldHide && hasBuff)
            buff.BuffOver();
    }

    static bool HasAdjacentOwnedShadow(Chess master)
    {
        Tile masterTile = master.moveController?.standTile;
        if (masterTile == null || MapManage.instance == null)
            return false;
        if (!master.skillController.context.TryGet(WisadelKeys.Shadows, out List<Chess> shadows)
            || shadows == null)
            return false;

        var neighborTiles = new HashSet<Tile>();
        foreach (Tile neighbor in MapManage.instance.NearTile(masterTile))
        {
            if (neighbor != null)
                neighborTiles.Add(neighbor);
        }

        for (int i = 0; i < shadows.Count; i++)
        {
            Chess shadow = shadows[i];
            if (shadow == null || shadow.IfDeath)
                continue;
            Tile shadowTile = shadow.moveController?.standTile;
            if (shadowTile != null && neighborTiles.Contains(shadowTile))
                return true;
        }

        return false;
    }
}
