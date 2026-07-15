using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 压力希被动：邻格（上下左右）友军每次攻击时，对该友军施加「压力」+1。
/// 自身不挂压力；定期刷新邻格订阅以免换位泄漏。
/// </summary>
[Serializable]
public class PassiveSkillEffect_YaliXi : ISkillEffect
{
    [SerializeReference]
    [Tooltip("友军尚无「压力」时挂上的模板")]
    public Buff_StressBuff_Death guestStressBuff;

    [Min(1)]
    public int stressPerAttack = 1;

    [Min(0.05f)]
    [Tooltip("刷新邻格订阅的间隔")]
    public float refreshInterval = 0.25f;

    static readonly Vector2Int[] Neighbor4 =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
    };

    static readonly List<Tile> TileScratch = new List<Tile>(4);
    static readonly List<Chess> AllyScratch = new List<Chess>(8);
    static readonly List<Chess> RemoveScratch = new List<Chess>(8);

    Chess _user;
    UnityAction<Chess> _onRemove;
    Timer _refreshTimer;
    readonly Dictionary<Chess, UnityAction<Chess>> _allyAttackHandlers = new Dictionary<Chess, UnityAction<Chess>>();

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;
        _user = user;

        _onRemove = OnRemove;
        user.OnRemove.AddListener(_onRemove);

        RefreshAllySubscriptions();
        if (GameManage.instance?.timerManage != null)
            _refreshTimer = GameManage.instance.timerManage.AddTimer(RefreshAllySubscriptions, refreshInterval, true);
    }

    void RefreshAllySubscriptions()
    {
        if (_user == null || _user.IfDeath)
            return;

        CollectNeighborAllies(_user, AllyScratch);

        RemoveScratch.Clear();
        foreach (var kv in _allyAttackHandlers)
        {
            Chess key = kv.Key;
            if (key == null || !AllyScratch.Contains(key))
                RemoveScratch.Add(key);
        }
        for (int i = 0; i < RemoveScratch.Count; i++)
        {
            Chess ally = RemoveScratch[i];
            if (ally == null)
                _allyAttackHandlers.Remove(ally);
            else
                UnsubscribeAlly(ally);
        }
        for (int i = 0; i < AllyScratch.Count; i++)
        {
            Chess ally = AllyScratch[i];
            if (ally == null || _allyAttackHandlers.ContainsKey(ally))
                continue;
            SubscribeAlly(ally);
        }
    }

    void SubscribeAlly(Chess ally)
    {
        if (ally?.equipWeapon == null) return;

        Chess captured = ally;
        UnityAction<Chess> handler = attacker => OnAllyAttack(captured, attacker);
        _allyAttackHandlers[ally] = handler;
        ally.equipWeapon.OnAttack.AddListener(handler);

        ally.OnRemove.AddListener(OnAllyRemoved);
    }

    void UnsubscribeAlly(Chess ally)
    {
        if (ally == null) return;
        if (!_allyAttackHandlers.TryGetValue(ally, out UnityAction<Chess> handler))
            return;

        if (ally.equipWeapon != null)
            ally.equipWeapon.OnAttack.RemoveListener(handler);
        ally.OnRemove.RemoveListener(OnAllyRemoved);
        _allyAttackHandlers.Remove(ally);
    }

    void OnAllyRemoved(Chess ally)
    {
        UnsubscribeAlly(ally);
    }

    void OnAllyAttack(Chess ally, Chess attacker)
    {
        if (_user == null || _user.IfDeath) return;
        if (ally == null || ally.IfDeath) return;
        if (attacker != ally) return;

        int delta = Mathf.Max(1, stressPerAttack);
        OkuwakiStressSpread.ApplyStressDelta(ally, delta, guestStressBuff);
    }

    static void CollectNeighborAllies(Chess user, List<Chess> into)
    {
        into.Clear();
        CollectNeighbor4Tiles(user.moveController?.standTile, TileScratch);
        for (int i = 0; i < TileScratch.Count; i++)
        {
            Tile tile = TileScratch[i];
            if (tile?.chessesIntile == null) continue;
            for (int c = 0; c < tile.chessesIntile.Count; c++)
            {
                Chess ally = tile.chessesIntile[c];
                if (ally == null || ally.IfDeath || ally == user)
                    continue;
                if (!ally.CompareTag(user.tag))
                    continue;
                if (!into.Contains(ally))
                    into.Add(ally);
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

    void ClearAllSubscriptions()
    {
        RemoveScratch.Clear();
        foreach (var kv in _allyAttackHandlers)
            RemoveScratch.Add(kv.Key);
        for (int i = 0; i < RemoveScratch.Count; i++)
        {
            Chess ally = RemoveScratch[i];
            if (ally != null)
                UnsubscribeAlly(ally);
        }
        _allyAttackHandlers.Clear();
    }

    void OnRemove(Chess chess)
    {
        if (_refreshTimer != null)
        {
            _refreshTimer.Stop();
            _refreshTimer = null;
        }
        ClearAllSubscriptions();

        if (_user != null && _onRemove != null)
            _user.OnRemove.RemoveListener(_onRemove);

        _user = null;
        _onRemove = null;
    }
}
