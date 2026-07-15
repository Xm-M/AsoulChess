using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 凑友希那被动：每次攻击时，对自身格子上下左右四邻格内的友方施加「压力」
/// （无则挂上；有则 BuffReset，默认每次 +1）。自身不挂压力。
/// </summary>
[Serializable]
public class PassiveSkillEffect_Yukina : ISkillEffect
{
    [SerializeReference]
    [Tooltip("邻格友方尚无「压力」时挂上的模板")]
    public Buff_StressBuff_Death guestStressBuff;

    [Min(1)]
    [Tooltip("每次攻击通过 BuffReset 增加的压力")]
    public int stressPerAttack = 1;

    static readonly Vector2Int[] Neighbor4 =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
    };

    static readonly List<Tile> TileScratch = new List<Tile>(4);

    Chess _user;
    UnityAction<Chess> _onAttack;
    UnityAction<Chess> _onRemove;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;
        _user = user;

        _onAttack = OnAttack;
        if (user.equipWeapon != null)
            user.equipWeapon.OnAttack.AddListener(_onAttack);

        _onRemove = OnRemove;
        user.OnRemove.AddListener(_onRemove);
    }

    void OnAttack(Chess attacker)
    {
        if (_user == null || attacker != _user || _user.IfDeath)
            return;

        CollectNeighbor4Tiles(_user.moveController?.standTile, TileScratch);
        int delta = Mathf.Max(1, stressPerAttack);
        for (int i = 0; i < TileScratch.Count; i++)
        {
            Tile tile = TileScratch[i];
            if (tile?.chessesIntile == null) continue;
            for (int c = 0; c < tile.chessesIntile.Count; c++)
            {
                Chess ally = tile.chessesIntile[c];
                if (ally == null || ally.IfDeath || ally == _user)
                    continue;
                if (!ally.CompareTag(_user.tag))
                    continue;
                OkuwakiStressSpread.ApplyStressDelta(ally, delta, guestStressBuff);
            }
        }
    }

    static void CollectNeighbor4Tiles(Tile center, List<Tile> buffer)
    {
        buffer.Clear();
        if (center == null || MapManage.instance == null) return;

        Vector2Int p = center.mapPos;
        MapManage map = MapManage.instance;
        for (int i = 0; i < Neighbor4.Length; i++)
        {
            int x = p.x + Neighbor4[i].x;
            int y = p.y + Neighbor4[i].y;
            if (!map.IfInMapRange(x, y)) continue;
            Tile t = map.tiles[x, y];
            if (t != null) buffer.Add(t);
        }
    }

    void OnRemove(Chess chess)
    {
        if (_user != null)
        {
            if (_onAttack != null && _user.equipWeapon != null)
                _user.equipWeapon.OnAttack.RemoveListener(_onAttack);
            if (_onRemove != null)
                _user.OnRemove.RemoveListener(_onRemove);
        }
        _user = null;
        _onAttack = null;
        _onRemove = null;
    }
}
