using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Doloris 被动：按武器 <see cref="IGridFindTarget"/> 范围每秒为友军施加 <see cref="Buff_Mujica_Doloris"/> 与 <see cref="Buff_Bard"/>；
/// 每 3 秒治疗范围内受伤且 HP 比例最低的友军（受伤的丰川祥子优先且治疗 +25%）。
/// </summary>
public class Passive_Mujica_Doloris : ISkillEffect
{
    public const string SakiChessName = "丰川祥子";

    [Tooltip("刷新友军 Buff 的间隔（秒）")]
    public float buffTickInterval = 1f;

    [Tooltip("治疗 tick 间隔（秒）")]
    public float healTickInterval = 3f;

    [SerializeReference]
    public Buff_Mujica_Doloris dolorisBuffTemplate;

    [SerializeReference]
    public Buff_Bard bardBuffTemplate;

    Chess _user;
    Timer _buffTimer;
    Timer _healTimer;
    public DamageMessege dm;
    readonly List<Chess> _allyScratch = new List<Chess>();

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null)
            return;
        _user = user;
        if (!(_user.equipWeapon?.weapon is Weapon_Sample ws) || !(ws.findTarget is IGridFindTarget grid))
            return;

        user.OnRemove.RemoveListener(OnOwnerRemove);
        user.OnRemove.AddListener(OnOwnerRemove);

        var tm = GameManage.instance?.timerManage;
        if (tm == null)
            return;

        OnBuffTick();
        OnHealTick();
        _buffTimer = tm.AddTimer(OnBuffTick, buffTickInterval, true);
        _healTimer = tm.AddTimer(OnHealTick, healTickInterval, true);
    }

    void OnOwnerRemove(Chess chess)
    {
        if (_buffTimer != null)
        {
            _buffTimer.Stop();
            _buffTimer = null;
        }
        if (_healTimer != null)
        {
            _healTimer.Stop();
            _healTimer = null;
        }
        _user = null;
    }

    void OnBuffTick()
    {
        if (_user == null || _user.IfDeath)
            return;
        if (!(_user.equipWeapon?.weapon is Weapon_Sample ws) || !(ws.findTarget is IGridFindTarget grid))
            return;

        MujicaDolorisGrid.CollectAlliesInAttackGrid(_user, grid, _allyScratch);
        Buff_Mujica_Doloris doloris = MakeDolorisBuff();
        Buff_Bard bard = MakeBardBuff();

        for (int i = 0; i < _allyScratch.Count; i++)
        {
            Chess ally = _allyScratch[i];
            if (ally == null || ally.IfDeath)
                continue;
            ally.buffController.AddBuff(doloris);
            ally.buffController.AddBuff(bard);
        }
    }

    void OnHealTick()
    {
        if (_user == null || _user.IfDeath)
            return;
        if (!(_user.equipWeapon?.weapon is Weapon_Sample ws) || !(ws.findTarget is IGridFindTarget grid))
            return;

        MujicaDolorisGrid.CollectAlliesInAttackGrid(_user, grid, _allyScratch);
        if (!MujicaDolorisGrid.TryPickHealTarget(_user, _allyScratch, out Chess healTarget, out bool sakiBonus))
            return;

        float atk = _user.propertyController.GetAttack();
        float amount = atk * 0.3f * (sakiBonus ? 1.25f : 1f);
        if (amount > 0f)
        {
            dm.damage = amount;
            dm.damageFrom = _user;
            dm.damageTo = healTarget;
            dm.damageType = DamageType.Heal;
            _user.propertyController.TakeDamage(dm);
        }
    }

    Buff_Mujica_Doloris MakeDolorisBuff()
    {
        Buff_Mujica_Doloris b = dolorisBuffTemplate != null
            ? (Buff_Mujica_Doloris)dolorisBuffTemplate.Clone()
            : new Buff_Mujica_Doloris();
        b.buffFrom = _user;
        b.damageRate = 0.3f;
        b.buffName = "Mujica_Doloris";
        return b;
    }

    Buff_Bard MakeBardBuff()
    {
        Buff_Bard b = bardBuffTemplate != null
            ? (Buff_Bard)bardBuffTemplate.Clone()
            : new Buff_Bard();
 
        b.healRate = 0.1f;
        b.buffName = "Bard_Doloris";
        return b;
    }
}

/// <summary>与 <see cref="IGridFindTarget"/> 共享格子几何，用于枚举友军 / 选治疗目标。</summary>
public static class MujicaDolorisGrid
{
    const int ColPoolSize = 1000;

    public static void CollectAlliesInAttackGrid(Chess user, IGridFindTarget grid, List<Chess> outAllies)
    {
        outAllies.Clear();
        if (user == null || grid == null || MapManage.instance == null)
            return;

        MapManage map = MapManage.instance;
        if (!GridFindTargetGeometry.TryGetBaseMapPos(user, map, out Vector2Int basePos))
            return;

        int forwardX = GridFindTargetGeometry.GetForwardX(user);
        LayerMask friendLayer = LayerMask.GetMask(user.tag);
        Collider2D[] cols = CheckObjectPoolManage.GetColArray(ColPoolSize);
        Vector2 ts = map.tileSize;

        if (grid.relativeCells == null)
        {
            CheckObjectPoolManage.ReleaseColArray(ColPoolSize, cols);
            return;
        }

        foreach (Vector2Int rel in grid.relativeCells)
        {
            int ax = basePos.x + rel.x * forwardX;
            int ay = basePos.y + rel.y;
            if (!GridFindTargetGeometry.IsDetectableCell(ax, ay, map.mapSize))
                continue;

            Tile tile = map.tiles[ax, ay];
            if (tile == null)
                continue;

            Vector2 center = GridFindTargetGeometry.GetCellOverlapCenter(tile, ts);
            int count = Physics2D.OverlapBoxNonAlloc(center, grid.boxHalfExtents, 0f, cols, friendLayer);
            for (int i = 0; i < count; i++)
            {
                if (cols[i] == null)
                    continue;
                Chess c = cols[i].GetComponent<Chess>();
                if (c == null || c.IfDeath || !c.CompareTag(user.tag))
                    continue;
                if (!outAllies.Contains(c))
                    outAllies.Add(c);
            }
        }

        CheckObjectPoolManage.ReleaseColArray(ColPoolSize, cols);
    }

    /// <summary>范围内受伤的丰川祥子优先；否则在受伤友军中选 HP/MaxHp 比例最低者。</summary>
    public static bool TryPickHealTarget(Chess doloris, List<Chess> alliesInRange, out Chess healTarget, out bool sakiBonus)
    {
        healTarget = null;
        sakiBonus = false;
        if (doloris == null || alliesInRange == null)
            return false;

        Chess injuredSaki = null;
        for (int i = 0; i < alliesInRange.Count; i++)
        {
            Chess c = alliesInRange[i];
            if (c == null || c.IfDeath || c == doloris)
                continue;
            var pc = c.propertyController;
            if (pc.GetHp() >= pc.GetMaxHp() - 0.0001f)
                continue;
            if (IsSaki(c))
            {
                injuredSaki = c;
                break;
            }
        }

        if (injuredSaki != null)
        {
            healTarget = injuredSaki;
            sakiBonus = true;
            return true;
        }

        float bestRatio = float.MaxValue;
        Chess best = null;
        for (int i = 0; i < alliesInRange.Count; i++)
        {
            Chess c = alliesInRange[i];
            if (c == null || c.IfDeath || c == doloris)
                continue;
            var pc = c.propertyController;
            float maxHp = pc.GetMaxHp();
            if (maxHp <= 0f)
                continue;
            if (pc.GetHp() >= maxHp - 0.0001f)
                continue;
            float ratio = pc.GetHp() / maxHp;
            if (ratio < bestRatio)
            {
                bestRatio = ratio;
                best = c;
            }
        }

        if (best != null)
        {
            healTarget = best;
            return true;
        }
        return false;
    }

    static bool IsSaki(Chess c)
    {
        string name = c.propertyController?.creator?.chessName;
        return !string.IsNullOrEmpty(name) && name.Contains("丰川祥子");
    }
}
