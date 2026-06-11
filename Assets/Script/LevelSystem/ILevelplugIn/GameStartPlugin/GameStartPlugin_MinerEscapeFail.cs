using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 「别放他们走」：矿工出土后（曾到达 x=0）再进入右边界（<see cref="MapManage_PVZ.deathTile"/>）则本局失败。
/// 掘进阶段从右向左，不会因此判负。
/// </summary>
public class GameStartPlugin_MinerEscapeFail : ILevelPlugin
{
    readonly HashSet<Chess> _surfacedMiners = new HashSet<Chess>();
    readonly Dictionary<Chess, UnityAction<Chess, Tile>> _reachHandlers = new Dictionary<Chess, UnityAction<Chess, Tile>>();

    public void StadgeEffect(LevelController levelController)
    {
        ClearState();
        EventController.Instance.AddListener<Chess>(EventName.WhenChessEnterWar.ToString(), OnChessEnterWar);
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), OnLeaveLevel);
    }

    public void OverPlugin(LevelController levelController)
    {
        OnLeaveLevel();
    }

    void OnLeaveLevel()
    {
        EventController.Instance.RemoveListener<Chess>(EventName.WhenChessEnterWar.ToString(), OnChessEnterWar);
        EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), OnLeaveLevel);
        foreach (var kv in _reachHandlers)
            UnbindMiner(kv.Key, kv.Value);
        ClearState();
    }

    void ClearState()
    {
        _surfacedMiners.Clear();
        _reachHandlers.Clear();
    }

    void OnChessEnterWar(Chess chess)
    {
        if (!IsMinerZombie(chess))
            return;
        BindMiner(chess);
    }

    void BindMiner(Chess chess)
    {
        if (chess?.moveController == null || _reachHandlers.ContainsKey(chess))
            return;

        UnityAction<Chess, Tile> handler = (c, tile) => OnMinerReachTile(c, tile);
        _reachHandlers[chess] = handler;
        chess.moveController.OnReachTile.AddListener(handler);
        chess.OnRemove.AddListener(OnChessRemoved);
    }

    void OnChessRemoved(Chess chess)
    {
        if (chess == null)
            return;
        if (_reachHandlers.TryGetValue(chess, out var handler))
            UnbindMiner(chess, handler);
        _surfacedMiners.Remove(chess);
        chess.OnRemove.RemoveListener(OnChessRemoved);
    }

    void UnbindMiner(Chess chess, UnityAction<Chess, Tile> handler)
    {
        if (chess?.moveController != null && handler != null)
            chess.moveController.OnReachTile.RemoveListener(handler);
        _reachHandlers.Remove(chess);
    }

    void OnMinerReachTile(Chess chess, Tile tile)
    {
        if (chess == null || chess.IfDeath || tile == null)
            return;
        if (LevelManage.instance == null || !LevelManage.instance.IfGameStart)
            return;

        if (tile.mapPos.x == 0)
        {
            _surfacedMiners.Add(chess);
            return;
        }

        if (!_surfacedMiners.Contains(chess))
            return;

        if (!IsRightBoundaryTile(tile))
            return;

        LevelManage.instance.GameOver(false);
    }

    static bool IsMinerZombie(Chess chess)
    {
        if (chess == null || chess.IfDeath || !chess.CompareTag("Enemy"))
            return false;
        if (chess.skillController?.passiveSkill is PassiveSkill passive)
            return passive.effect is PassiveSkillEffect_MinerZombie;
        return false;
    }

    static bool IsRightBoundaryTile(Tile tile)
    {
        var mapPvz = MapManage.instance as MapManage_PVZ;
        if (mapPvz?.deathTile != null && tile == mapPvz.deathTile)
            return true;

        var map = MapManage.instance;
        if (map == null)
            return false;
        return tile.mapPos.x >= map.mapSize.x;
    }
}
