using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 蹦极僵尸被动：入场时清空 <see cref="targetSprites"/> 上所有 sprite（尚未抱走）；
/// 再从所有敌方中随机选一名满足「体型小于自身」且「无 <see cref="Buff_Zombie_BullyBuff"/>」的单位，
/// 瞬移到其所在格、对其施加霸凌 Buff；同时在其所在格为中心的 3×3 内收集带 <see cref="Buff_BloodLine"/> 的敌方（与主目标去重），
/// 整份名单最多 9 个，写入 <see cref="SkillContext"/> 键 <see cref="BungeeSkillContextKeys.Victims"/> 与 <c>霸凌目标</c>（主目标）。
/// 下跳/上拉由动画驱动，本逻辑不负责位移演出。
/// </summary>
public class PassiveSkill_Bungee : ISkillEffect
{
    public const int MaxKidnapVictims = 9;

    [SerializeReference, LabelText("霸凌 Buff 模板")]
    public Buff_Zombie_BullyBuff bullyBuff;

    [LabelText("抱走展示用 SpriteRenderer 列表")]
    [Tooltip("与主动 SkillEffect_Bungee 上列表一一对应；入场清空 sprite，主动时按 Victims 写入外观与位置。")]
    public List<SpriteRenderer> targetSprites = new List<SpriteRenderer>();

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || user.IfDeath || bullyBuff == null || ChessTeamManage.Instance == null) return;

        ClearHandSprites();

        var enemies = ChessTeamManage.Instance.GetEnemyTeam(user.tag);
        if (enemies == null || enemies.Count == 0) return;

        int mySize = user.propertyController.GetSize();
        var candidates = new List<Chess>();
        for (int i = 0; i < enemies.Count; i++)
        {
            var e = enemies[i];
            if (e == null || e == user || e.IfDeath) continue;
            if (e.propertyController.GetSize() >= mySize) continue;
            if (HasBullyBuff(e)) continue;
            candidates.Add(e);
        }

        if (candidates.Count == 0) return;

        Chess target = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        var targetTile = target.moveController?.standTile;
        if (targetTile == null) return;

        var oldTile = user.moveController?.standTile;
        if (oldTile != null)
            oldTile.ChessLeave(user);
        targetTile.ChessEnter(user);

        target.buffController.AddBuff(bullyBuff);
        user.skillController.context.Set<Chess>("霸凌目标", target);

        var victims = BuildKidnapList(target, user, enemies);
        user.skillController.context.Set<List<Chess>>(BungeeSkillContextKeys.Victims, victims);
    }

    void ClearHandSprites()
    {
        if (targetSprites == null) return;
        for (int i = 0; i < targetSprites.Count; i++)
        {
            if (targetSprites[i] != null)
                targetSprites[i].sprite = null;
        }
    }

    /// <summary>主目标优先，再按 3×3 扫描顺序加入带血线连结 Buff 的敌方，总数不超过 <see cref="MaxKidnapVictims"/>。</summary>
    static List<Chess> BuildKidnapList(Chess primary, Chess user, List<Chess> enemies)
    {
        var victims = new List<Chess>(MaxKidnapVictims);
        void TryAdd(Chess c)
        {
            if (c == null || c.IfDeath || c == user || victims.Count >= MaxKidnapVictims) return;
            if (victims.Contains(c)) return;
            if (!IsInEnemyList(c, enemies)) return;
            victims.Add(c);
        }

        TryAdd(primary);

        var center = primary.moveController?.standTile;
        if (center == null || MapManage.instance == null) return victims;

        var tiles = CollectTiles3x3(center);
        for (int ti = 0; ti < tiles.Count && victims.Count < MaxKidnapVictims; ti++)
        {
            var tile = tiles[ti];
            if (tile?.chessesIntile == null) continue;
            for (int ci = 0; ci < tile.chessesIntile.Count && victims.Count < MaxKidnapVictims; ci++)
            {
                var c = tile.chessesIntile[ci];
                if (!HasBloodLineBuff(c)) continue;
                TryAdd(c);
            }
        }

        return victims;
    }

    static List<Tile> CollectTiles3x3(Tile center)
    {
        var list = new List<Tile>(9);
        var p = center.mapPos;
        var map = MapManage.instance;
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int x = p.x + dx, y = p.y + dy;
                if (!map.IfInMapRange(x, y)) continue;
                var t = map.tiles[x, y];
                if (t != null)
                    list.Add(t);
            }
        }
        return list;
    }

    static bool IsInEnemyList(Chess c, List<Chess> enemies)
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == c)
                return true;
        }
        return false;
    }

    static bool HasBloodLineBuff(Chess chess)
    {
        return chess != null
            && chess.buffController != null
            && chess.buffController.buffDic != null
            && chess.buffController.buffDic.ContainsKey(Buff_BloodLine.BuffKey);
    }

    static bool HasBullyBuff(Chess chess)
    {
        if (chess?.buffController?.buffDic == null) return false;
        foreach (var kv in chess.buffController.buffDic)
        {
            if (kv.Value is Buff_Zombie_BullyBuff)
                return true;
        }
        return false;
    }
}
