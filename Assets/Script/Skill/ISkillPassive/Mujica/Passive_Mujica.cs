using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 按 <see cref="PropertyCreator"/> 缓存「该卡首次成功采样」的普攻 <c>relativeCells</c> 拷贝，避免反复读运行时已被合并改写的 <see cref="Weapon_Sample.findTarget"/>；
/// 不调用 <see cref="PropertyCreator.GetPre"/>，避免 SerializeReference 大图解析栈溢出。
/// </summary>
static class MujicaAveGridBaseCache
{
    static bool _leaveSubscribed;
    static readonly Dictionary<int, List<Vector2Int>> ByCreatorInstanceId = new Dictionary<int, List<Vector2Int>>();

    public static void EnsureLeaveLevelClearsCache()
    {
        if (_leaveSubscribed)
            return;
        _leaveSubscribed = true;
        if (EventController.Instance != null)
            EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), Clear);
    }

    static void Clear() => ByCreatorInstanceId.Clear();

    /// <summary>每种 PropertyCreator 只记第一次写入的格子列表。</summary>
    public static void TryRegisterSnapshot(PropertyCreator creator, IList<Vector2Int> baseCells)
    {
        if (creator == null || baseCells == null || baseCells.Count == 0)
            return;
        int id = creator.GetInstanceID();
        if (ByCreatorInstanceId.ContainsKey(id))
            return;
        var copy = new List<Vector2Int>(baseCells.Count);
        for (int i = 0; i < baseCells.Count; i++)
            copy.Add(baseCells[i]);
        ByCreatorInstanceId[id] = copy;
    }

    public static IList<Vector2Int> TryGetSnapshot(PropertyCreator creator)
    {
        if (creator == null)
            return null;
        return ByCreatorInstanceId.TryGetValue(creator.GetInstanceID(), out var list) ? list : null;
    }
}

/// <summary>
/// Oblivionis：维护 <see cref="_mujicaGridDir"/>（队友 → 其普攻相对格拷贝），与自身底形取并集后写入 <see cref="Weapon_Sample.findTarget"/>。
/// 「在攻击范围内」= 队友站立格落在自身当前普攻覆盖的地图格集合内；「右方」= 同队队友（沿用现有队伍遍历）。
/// </summary>
public class Passive_Mujica_Oblivionis : ISkillEffect
{
    public const string AveMujicaTag = "AveMujica";

    [Tooltip("重新计算并集并更新索敌的间隔（秒）")]
    public float tickInterval = 1f;

    /// <summary>友方 → 该单位普攻相对格拷贝（相对该单位自身）。仅运行时。</summary>
    [System.NonSerialized]
    Dictionary<Chess, List<Vector2Int>> _mujicaGridDir = new Dictionary<Chess, List<Vector2Int>>();

    [System.NonSerialized]
    Dictionary<Chess, UnityAction<Chess>> _mujicaAllyRemoveHandlers = new Dictionary<Chess, UnityAction<Chess>>();

    [System.NonSerialized]
    List<Chess> _keysScratch = new List<Chess>();

    Chess _user;
    Timer _timer;
    Weapon_Sample _weapon;
    IGridFindTarget _originalGrid;
    readonly List<Vector2Int> _baseRelativeCells = new List<Vector2Int>();
    Vector2 _boxHalfExtents;
    bool _recomputing;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;
        _mujicaGridDir ??= new Dictionary<Chess, List<Vector2Int>>();
        _mujicaAllyRemoveHandlers ??= new Dictionary<Chess, UnityAction<Chess>>();
        _keysScratch ??= new List<Chess>();
        _user = user;
        _weapon = user.equipWeapon?.weapon as Weapon_Sample;
        _originalGrid = _weapon?.findTarget as IGridFindTarget;
        if (_weapon == null || _originalGrid == null)
            return;

        _baseRelativeCells.Clear();
        if (_originalGrid.relativeCells != null)
            _baseRelativeCells.AddRange(_originalGrid.relativeCells);
        _boxHalfExtents = _originalGrid.boxHalfExtents;

        MujicaAveGridBaseCache.EnsureLeaveLevelClearsCache();
        MujicaAveGridBaseCache.TryRegisterSnapshot(user.propertyController?.creator, _baseRelativeCells);

        user.OnRemove.RemoveListener(OnOwnerRemove);
        user.OnRemove.AddListener(OnOwnerRemove);
        if (_timer != null)
        {
            _timer.Stop();
            _timer = null;
        }
        RecomputeMergedFindTarget();
        if (GameManage.instance != null && GameManage.instance.timerManage != null)
            _timer = GameManage.instance.timerManage.AddTimer(RecomputeMergedFindTarget, tickInterval, true);
    }

    void OnOwnerRemove(Chess chess)
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer = null;
        }
        ClearMujicaGridDirAndHandlers();
        if (_weapon != null && _originalGrid != null)
            _weapon.findTarget = _originalGrid;
        _weapon = null;
        _originalGrid = null;
        _user = null;
    }

    void ClearMujicaGridDirAndHandlers()
    {
        foreach (var kv in _mujicaAllyRemoveHandlers)
        {
            if (kv.Key != null)
                kv.Key.OnRemove.RemoveListener(kv.Value);
        }
        _mujicaAllyRemoveHandlers.Clear();
        _mujicaGridDir.Clear();
    }

    void RemoveTrackedAlly(Chess ally)
    {
        if (ally == null)
            return;
        _mujicaGridDir.Remove(ally);
        if (_mujicaAllyRemoveHandlers.TryGetValue(ally, out UnityAction<Chess> handler))
        {
            ally.OnRemove.RemoveListener(handler);
            _mujicaAllyRemoveHandlers.Remove(ally);
        }
    }

    void RecomputeMergedFindTarget()
    {
        if (_recomputing)
            return;
        _recomputing = true;
        try
        {
            if (_user == null || _weapon == null || _originalGrid == null)
                return;
            if (_user.IfDeath)
                return;

            MapManage map = MapManage.instance;
            if (map == null)
                return;

            if (!GridFindTargetGeometry.TryGetBaseMapPos(_user, map, out Vector2Int uBase))
            {
                _weapon.findTarget = _originalGrid;
                return;
            }

            int uForward = GridFindTargetGeometry.GetForwardX(_user);

            var scanCells = new HashSet<Vector2Int>();
            AddDetectableMapCells(uBase, uForward, _baseRelativeCells, map.mapSize, scanCells);

            List<Chess> team = ChessTeamManage.Instance?.GetTeam(_user.tag);

            _keysScratch.Clear();
            foreach (Chess key in _mujicaGridDir.Keys)
                _keysScratch.Add(key);
            for (int i = 0; i < _keysScratch.Count; i++)
            {
                Chess tracked = _keysScratch[i];
                if (!ShouldTrackAlly(tracked, team, scanCells))
                    RemoveTrackedAlly(tracked);
            }

            if (team != null)
            {
                for (int i = 0; i < team.Count; i++)
                {
                    Chess ally = team[i];
                    if (ally == null || ally == _user || ally.IfDeath)
                        continue;
                    if (!IsAllyEligibleForExtension(ally, scanCells))
                        continue;
                    if (_mujicaGridDir.ContainsKey(ally))
                        continue;

                    IList<Vector2Int> src = ResolveAllyBaseRelativeCells(ally);
                    if (src == null || src.Count == 0)
                        continue;

                    var copy = new List<Vector2Int>(src.Count);
                    for (int r = 0; r < src.Count; r++)
                        copy.Add(src[r]);
                    _mujicaGridDir[ally] = copy;

                    Chess captured = ally;
                    UnityAction<Chess> handler = removed =>
                    {
                        if (removed != captured)
                            return;
                        RemoveTrackedAlly(captured);
                    };
                    _mujicaAllyRemoveHandlers[captured] = handler;
                    captured.OnRemove.AddListener(handler);
                }
            }

            var mergedRelative = new HashSet<Vector2Int>(_baseRelativeCells);
            var baseSet = new HashSet<Vector2Int>(_baseRelativeCells);

            foreach (var kv in _mujicaGridDir)
            {
                Chess ally = kv.Key;
                List<Vector2Int> pattern = kv.Value;
                if (ally == null || ally.IfDeath || pattern == null || pattern.Count == 0)
                    continue;
                if (!GridFindTargetGeometry.TryGetBaseMapPos(ally, map, out Vector2Int aBase))
                    continue;
                int aForward = GridFindTargetGeometry.GetForwardX(ally);

                for (int r = 0; r < pattern.Count; r++)
                {
                    Vector2Int relA = pattern[r];
                    int mx = aBase.x + relA.x * aForward;
                    int my = aBase.y + relA.y;
                    if (!GridFindTargetGeometry.IsDetectableCell(mx, my, map.mapSize))
                        continue;
                    int relUx = (mx - uBase.x) * uForward;
                    int relUy = my - uBase.y;
                    mergedRelative.Add(new Vector2Int(relUx, relUy));
                }
            }

            if (SetsEqual(mergedRelative, baseSet))
                _weapon.findTarget = _originalGrid;
            else
            {
                var grid = new IGridFindTarget
                {
                    boxHalfExtents = _boxHalfExtents,
                    relativeCells = new List<Vector2Int>(mergedRelative)
                };
                _weapon.findTarget = grid;
            }
        }
        finally
        {
            _recomputing = false;
        }
    }

    static bool IsAllyEligibleForExtension(Chess ally, HashSet<Vector2Int> scanCells)
    {
        if (ally.moveController?.standTile == null)
            return false;
        List<string> tags = ally.propertyController?.creator?.plantTags;
        if (tags == null || !tags.Contains(AveMujicaTag))
            return false;
        if (!scanCells.Contains(ally.moveController.standTile.mapPos))
            return false;
        if (!(ally.equipWeapon?.weapon is Weapon_Sample ws) || !(ws.findTarget is IGridFindTarget))
            return false;
        return true;
    }

    static bool ShouldTrackAlly(Chess tracked, List<Chess> team, HashSet<Vector2Int> scanCells)
    {
        if (tracked == null || tracked.IfDeath)
            return false;
        if (team == null || !team.Contains(tracked))
            return false;
        return IsAllyEligibleForExtension(tracked, scanCells);
    }

    static IList<Vector2Int> ResolveAllyBaseRelativeCells(Chess ally)
    {
        PropertyCreator creator = ally.propertyController?.creator;
        IList<Vector2Int> snap = MujicaAveGridBaseCache.TryGetSnapshot(creator);
        if (snap != null)
            return snap;
        if (ally.equipWeapon?.weapon is Weapon_Sample liveWs && liveWs.findTarget is IGridFindTarget liveG && liveG.relativeCells != null && liveG.relativeCells.Count > 0)
        {
            var copy = new List<Vector2Int>(liveG.relativeCells.Count);
            for (int i = 0; i < liveG.relativeCells.Count; i++)
                copy.Add(liveG.relativeCells[i]);
            MujicaAveGridBaseCache.TryRegisterSnapshot(creator, copy);
            return copy;
        }
        return null;
    }

    static bool SetsEqual(HashSet<Vector2Int> a, HashSet<Vector2Int> b)
    {
        if (a.Count != b.Count)
            return false;
        foreach (Vector2Int v in a)
        {
            if (!b.Contains(v))
                return false;
        }
        return true;
    }

    static void AddDetectableMapCells(Vector2Int basePos, int forwardX, IList<Vector2Int> relativeCells, Vector2Int mapSize, HashSet<Vector2Int> outCells)
    {
        if (relativeCells == null)
            return;
        for (int i = 0; i < relativeCells.Count; i++)
        {
            Vector2Int rel = relativeCells[i];
            int ax = basePos.x + rel.x * forwardX;
            int ay = basePos.y + rel.y;
            if (!GridFindTargetGeometry.IsDetectableCell(ax, ay, mapSize))
                continue;
            outCells.Add(new Vector2Int(ax, ay));
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Scene 视图用红色线框绘制当前 <see cref="Weapon_Sample.findTarget"/> 的格子范围（与 <see cref="IGridFindTarget.DrawGizmos"/> 一致，颜色为红）。
    /// 若该棋子被动不是 Oblivionis 则返回 <c>false</c>，由 <see cref="Chess.OnDrawGizmos"/> 回退为默认青色绘制。
    /// </summary>
    public static bool TryDrawAttackRangeGizmos(Chess user)
    {
        if (user == null)
            return false;
        if (!(user.skillController?.passiveSkill is PassiveSkill ps) || !(ps.effect is Passive_Mujica_Oblivionis))
            return false;
        if (!(user.equipWeapon?.weapon is Weapon_Sample ws) || !(ws.findTarget is IGridFindTarget grid))
            return false;

        MapManage map = MapManage.instance;
        if (map == null && !Application.isPlaying)
            map = UnityEngine.Object.FindObjectOfType<MapManage>();
        if (map == null)
            return true;

        if (!GridFindTargetGeometry.TryGetBaseMapPos(user, map, out Vector2Int basePos))
            return true;

        int forwardX = GridFindTargetGeometry.GetForwardX(user);
        Vector2 ts = map.tileSize;
        if (grid.relativeCells == null)
            return true;

        Color prev = Gizmos.color;
        Gizmos.color = Color.red;

        foreach (Vector2Int rel in grid.relativeCells)
        {
            int ax = basePos.x + rel.x * forwardX;
            int ay = basePos.y + rel.y;
            if (!GridFindTargetGeometry.IsDetectableCell(ax, ay, map.mapSize))
                continue;

            Tile tile = map.tiles[ax, ay];
            if (tile == null)
                continue;

            Vector2 c = GridFindTargetGeometry.GetCellOverlapCenter(tile, ts);
            Vector3 center = new Vector3(c.x, c.y, 0f);
            Vector3 size = new Vector3(grid.boxHalfExtents.x * 2f, grid.boxHalfExtents.y * 2f, 0.05f);
            Gizmos.DrawWireCube(center, size);
        }

        Gizmos.color = prev;
        return true;
    }
#endif
}

public class Passive_Mujica_Mortis : ISkillEffect
{
    [SerializeReference]
    public Buff ResumeBuff;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        user.propertyController.onGetDamage.AddListener(OnGetDamage);
    }
    public void OnGetDamage(DamageMessege dm)
    {
        Chess user = dm.damageTo;
        if (user.propertyController.GetHp() <= 0)
        {
            user.propertyController.ChangeHp(1);
            user.stateController.ChangeState(StateName.ResumeState);
            user.GetComponent<AudioPlayer>().PlaySubN(1);
            user.buffController.AddBuff(ResumeBuff);
        }
    }
}
