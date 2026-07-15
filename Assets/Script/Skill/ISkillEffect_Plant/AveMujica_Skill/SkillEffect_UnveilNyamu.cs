using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 揭幕喵梦：对自身 3×3（含自己）范围内所有友军施加压力，并在每名友军位置生成阳光。
/// 阳光数量取 <see cref="SkillConfig.baseDamage"/>[0]（默认 25）；压力为 <see cref="stressAmount"/>。
/// </summary>
[Serializable]
public class SkillEffect_UnveilNyamu : ISkillEffect
{
    [SerializeReference]
    [Tooltip("友军尚无「压力」时挂上的模板")]
    public Buff_StressBuff_Death guestStressBuff;

    [Min(1)]
    public int stressAmount = 25;

    [Tooltip("阳光生成位置相对友军格子的偏移")]
    public Vector3 sunOffset = new Vector3(0f, 1f, 0f);

    static readonly List<Tile> TileScratch = new List<Tile>(9);

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;

        int sunAmount = 25;
        if (config?.baseDamage != null && config.baseDamage.Count > 0)
            sunAmount = Mathf.Max(0, (int)config.baseDamage[0]);

        Collect3x3Tiles(user.moveController?.standTile, TileScratch);
        string allyTag = user.tag;

        for (int ti = 0; ti < TileScratch.Count; ti++)
        {
            Tile tile = TileScratch[ti];
            if (tile?.chessesIntile == null) continue;

            for (int ci = 0; ci < tile.chessesIntile.Count; ci++)
            {
                Chess ally = tile.chessesIntile[ci];
                if (ally == null || ally.IfDeath) continue;
                if (!ally.CompareTag(allyTag)) continue;

                OkuwakiStressSpread.ApplyStressDelta(ally, stressAmount, guestStressBuff);

                if (sunAmount <= 0) continue;
                if (UIManage.GetView<ItemPanel>() == null) continue;

                SunLight light = UIManage.GetView<ItemPanel>().Create<SunLight>() as SunLight;
                if (light == null) continue;

                Vector3 pos = tile.transform.position + sunOffset;
                light.InitSunLight(tile, sunAmount, pos);
            }
        }
    }

    static void Collect3x3Tiles(Tile center, List<Tile> buffer)
    {
        buffer.Clear();
        if (center == null || MapManage.instance == null) return;

        var p = center.mapPos;
        var map = MapManage.instance;
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int x = p.x + dx;
                int y = p.y + dy;
                if (!map.IfInMapRange(x, y)) continue;
                Tile t = map.tiles[x, y];
                if (t != null) buffer.Add(t);
            }
        }
    }
}
