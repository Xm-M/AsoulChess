using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 按 <see cref="PropertyCreator"/> 缓存「该卡首次进场时」的普攻格子（relativeCells），避免读 <see cref="PropertyCreator.GetPre"/> 预制体上的 SerializeReference 大图导致栈溢出；
/// 并避免两名 Oblivionis 互读对方已合并的 runtime <see cref="Weapon_Sample.findTarget"/>。
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

    /// <summary>每种 PropertyCreator 只记第一次写入的格子（进场时尚未被延伸被动改写）。</summary>
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
/// Oblivionis：用运行时字典记录「已进入延伸条件」的右方单位及其攻击格子模板（相对该队友的 <c>relativeCells</c> 拷贝）；
/// 每次 <see cref="RecomputeMergedFindTarget"/> 先同步字典（不再满足条件则从字典移除并取消 <c>OnRemove</c>），再对字典内单位按当前站位映射到自身相对格，与自身底形取并集。
/// 队友离场时由其 <c>OnRemove</c> 从字典移除。延伸形状来源仍用 <see cref="MujicaAveGridBaseCache"/>，避免读预制体 <c>GetPre()</c>。
/// 字典勿用 <c>public</c> 暴露给 Odin/SerializeReference，否则会触发编辑器 <c>ArrayTypeMismatchException</c>。
/// </summary>
public class Passive_Mujica_Oblivionis : ISkillEffect
{
    //public const string AveMujicaTag = "AveMujica";

    //[Tooltip("重新计算并集并更新索敌的间隔（秒）")]
    //public float tickInterval = 1f;

    ///// <summary>右方单位 → 该单位普攻 <see cref="IGridFindTarget.relativeCells"/> 的拷贝（相对该单位自身）。仅运行时，不参与序列化。</summary>
    //[HideInInspector]
    //[System.NonSerialized]
    //Dictionary<Chess, List<Vector2Int>> _mujicaGridDir = new Dictionary<Chess, List<Vector2Int>>();

    //[HideInInspector]
    //[System.NonSerialized]
    //Dictionary<Chess, UnityAction<Chess>> _mujicaAllyRemoveHandlers = new Dictionary<Chess, UnityAction<Chess>>();

    //[HideInInspector]
    //[System.NonSerialized]
    //List<Chess> _keysScratch = new List<Chess>();

    //Chess _user;
    //Timer _timer;
    //Weapon_Sample _weapon;
    //IGridFindTarget _originalGrid;
    //readonly List<Vector2Int> _baseRelativeCells = new List<Vector2Int>();
    //Vector2 _boxHalfExtents;
    //bool _recomputing;

    //public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    //{
    //    if (user == null) return;
    //    _mujicaGridDir ??= new Dictionary<Chess, List<Vector2Int>>();
    //    _mujicaAllyRemoveHandlers ??= new Dictionary<Chess, UnityAction<Chess>>();
    //    _keysScratch ??= new List<Chess>();
    //    _user = user;
    //    _weapon = user.equipWeapon?.weapon as Weapon_Sample;
    //    _originalGrid = _weapon?.findTarget as IGridFindTarget;
    //    if (_weapon == null || _originalGrid == null)
    //        return;

    //    _baseRelativeCells.Clear();
    //    if (_originalGrid.relativeCells != null)
    //        _baseRelativeCells.AddRange(_originalGrid.relativeCells);
    //    _boxHalfExtents = _originalGrid.boxHalfExtents;

    //    MujicaAveGridBaseCache.EnsureLeaveLevelClearsCache();
    //    MujicaAveGridBaseCache.TryRegisterSnapshot(user.propertyController?.creator, _baseRelativeCells);

    //    user.OnRemove.RemoveListener(OnOwnerRemove);
    //    user.OnRemove.AddListener(OnOwnerRemove);
    //    if (_timer != null)
    //    {
    //        _timer.Stop();
    //        _timer = null;
    //    }
    //    RecomputeMergedFindTarget();
    //    if (GameManage.instance != null && GameManage.instance.timerManage != null)
    //        _timer = GameManage.instance.timerManage.AddTimer(RecomputeMergedFindTarget, tickInterval, true);
    //}

    //void OnOwnerRemove(Chess chess)
    //{
    //    if (_timer != null)
    //    {
    //        _timer.Stop();
    //        _timer = null;
    //    }
    //    ClearMujicaGridDirAndHandlers();
    //    if (_weapon != null && _originalGrid != null)
    //        _weapon.findTarget = _originalGrid;
    //    _weapon = null;
    //    _originalGrid = null;
    //    _user = null;
    //}

    //void ClearMujicaGridDirAndHandlers()
    //{
    //    foreach (var kv in _mujicaAllyRemoveHandlers)
    //    {
    //        if (kv.Key != null)
    //            kv.Key.OnRemove.RemoveListener(kv.Value);
    //    }
    //    _mujicaAllyRemoveHandlers.Clear();
    //    _mujicaGridDir.Clear();
    //}

    //void RemoveTrackedAlly(Chess ally)
    //{
    //    if (ally == null)
    //        return;
    //    _mujicaGridDir.Remove(ally);
    //    if (_mujicaAllyRemoveHandlers.TryGetValue(ally, out UnityAction<Chess> handler))
    //    {
    //        ally.OnRemove.RemoveListener(handler);
    //        _mujicaAllyRemoveHandlers.Remove(ally);
    //    }
    //}

    //void RecomputeMergedFindTarget()
    //{
    //    if (_recomputing)
    //        return;
    //    _recomputing = true;
    //    try
    //    {
    //        if (_user == null || _weapon == null || _originalGrid == null)
    //            return;
    //        if (_user.IfDeath)
    //            return;

    //        MapManage map = MapManage.instance;
    //        if (map == null)
    //            return;

    //        if (!GridFindTargetGeometry.TryGetBaseMapPos(_user, map, out Vector2Int uBase))
    //        {
    //            _weapon.findTarget = _originalGrid;
    //            return;
    //        }

    //        int uForward = GridFindTargetGeometry.GetForwardX(_user);

    //        var scanCells = new HashSet<Vector2Int>();
    //        AddDetectableMapCells(uBase, uForward, _baseRelativeCells, map.mapSize, scanCells);

    //        List<Chess> team = ChessTeamManage.Instance?.GetTeam(_user.tag);

    //        // 1) 同步字典：已跟踪单位若不再满足条件则移除（含出攻击范围）
    //        _keysScratch.Clear();
    //        foreach (Chess key in _mujicaGridDir.Keys)
    //            _keysScratch.Add(key);
    //        for (int i = 0; i < _keysScratch.Count; i++)
    //        {
    //            Chess tracked = _keysScratch[i];
    //            if (!ShouldTrackAlly(tracked, team, scanCells))
    //                RemoveTrackedAlly(tracked);
    //        }

    //        // 2) 遍历右方：满足条件且不在字典则登记 + OnRemove
    //        if (team != null)
    //        {
    //            for (int i = 0; i < team.Count; i++)
    //            {
    //                Chess ally = team[i];
    //                if (ally == null || ally == _user || ally.IfDeath)
    //                    continue;
    //                if (!IsAllyEligibleForExtension(ally, scanCells))
    //                    continue;
    //                if (_mujicaGridDir.ContainsKey(ally))
    //                    continue;

    //                IList<Vector2Int> src = ResolveAllyBaseRelativeCells(ally);
    //                if (src == null || src.Count == 0)
    //                    continue;

    //                var copy = new List<Vector2Int>(src.Count);
    //                for (int r = 0; r < src.Count; r++)
    //                    copy.Add(src[r]);
    //                _mujicaGridDir[ally] = copy;

    //                Chess captured = ally;
    //                UnityAction<Chess> handler = removed =>
    //                {
    //                    if (removed != captured)
    //                        return;
    //                    RemoveTrackedAlly(captured);
    //                };
    //                _mujicaAllyRemoveHandlers[captured] = handler;
    //                captured.OnRemove.AddListener(handler);
    //            }
    //        }

    //        // 3) 并集：自身底形 + 字典内各单位当前站位下的格子 → 自身 relativeCells
    //        var mergedRelative = new HashSet<Vector2Int>(_baseRelativeCells);
    //        var baseSet = new HashSet<Vector2Int>(_baseRelativeCells);

    //        foreach (var kv in _mujicaGridDir)
    //        {
    //            Chess ally = kv.Key;
    //            List<Vector2Int> pattern = kv.Value;
    //            if (ally == null || ally.IfDeath || pattern == null || pattern.Count == 0)
    //                continue;
    //            if (!GridFindTargetGeometry.TryGetBaseMapPos(ally, map, out Vector2Int aBase))
    //                continue;
    //            int aForward = GridFindTargetGeometry.GetForwardX(ally);

    //            for (int r = 0; r < pattern.Count; r++)
    //            {
    //                Vector2Int relA = pattern[r];
    //                int mx = aBase.x + relA.x * aForward;
    //                int my = aBase.y + relA.y;
    //                if (!GridFindTargetGeometry.IsDetectableCell(mx, my, map.mapSize))
    //                    continue;
    //                int relUx = (mx - uBase.x) * uForward;
    //                int relUy = my - uBase.y;
    //                mergedRelative.Add(new Vector2Int(relUx, relUy));
    //            }
    //        }

    //        if (SetsEqual(mergedRelative, baseSet))
    //            _weapon.findTarget = _originalGrid;
    //        else
    //        {
    //            var grid = new IGridFindTarget
    //            {
    //                boxHalfExtents = _boxHalfExtents,
    //                relativeCells = new List<Vector2Int>(mergedRelative)
    //            };
    //            _weapon.findTarget = grid;
    //        }
    //    }
    //    finally
    //    {
    //        _recomputing = false;
    //    }
    //}

    //static bool IsAllyEligibleForExtension(Chess ally, HashSet<Vector2Int> scanCells)
    //{
    //    if (ally.moveController?.standTile == null)
    //        return false;
    //    List<string> tags = ally.propertyController?.creator?.plantTags;
    //    if (tags == null || !tags.Contains(AveMujicaTag))
    //        return false;
    //    if (!scanCells.Contains(ally.moveController.standTile.mapPos))
    //        return false;
    //    if (!(ally.equipWeapon?.weapon is Weapon_Sample ws) || !(ws.findTarget is IGridFindTarget))
    //        return false;
    //    return true;
    //}

    //static bool ShouldTrackAlly(Chess tracked, List<Chess> team, HashSet<Vector2Int> scanCells)
    //{
    //    if (tracked == null || tracked.IfDeath)
    //        return false;
    //    if (team == null || !team.Contains(tracked))
    //        return false;
    //    return IsAllyEligibleForExtension(tracked, scanCells);
    //}

    //static IList<Vector2Int> ResolveAllyBaseRelativeCells(Chess ally)
    //{
    //    PropertyCreator creator = ally.propertyController?.creator;
    //    IList<Vector2Int> snap = MujicaAveGridBaseCache.TryGetSnapshot(creator);
    //    if (snap != null)
    //        return snap;
    //    if (ally.equipWeapon?.weapon is Weapon_Sample liveWs && liveWs.findTarget is IGridFindTarget liveG)
    //        return liveG.relativeCells;
    //    return null;
    //}

    //static bool SetsEqual(HashSet<Vector2Int> a, HashSet<Vector2Int> b)
    //{
    //    if (a.Count != b.Count)
    //        return false;
    //    foreach (Vector2Int v in a)
    //    {
    //        if (!b.Contains(v))
    //            return false;
    //    }
    //    return true;
    //}

    //static void AddDetectableMapCells(Vector2Int basePos, int forwardX, IList<Vector2Int> relativeCells, Vector2Int mapSize, HashSet<Vector2Int> outCells)
    //{
    //    if (relativeCells == null)
    //        return;
    //    for (int i = 0; i < relativeCells.Count; i++)
    //    {
    //        Vector2Int rel = relativeCells[i];
    //        int ax = basePos.x + rel.x * forwardX;
    //        int ay = basePos.y + rel.y;
    //        if (!GridFindTargetGeometry.IsDetectableCell(ax, ay, mapSize))
    //            continue;
    //        outCells.Add(new Vector2Int(ax, ay));
    //    }
    //}
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        throw new System.NotImplementedException();
    }
}

public class Passive_Mujica_Mortis : ISkillEffect
{
    [SerializeReference]
    public Buff ResumeBuff;
    public   void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
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
