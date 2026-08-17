using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 羽川翼被动：常态产阳（由 ColdSkill 负责）；压力 ≥ 阈值自动黑羽川；
/// 黑形态 HP 加成、吸血、大范围索敌移动近战、压力每秒下降；归 0 回原格，无法回去则死亡。
/// 进/出黑形态分别播放 <c>transform</c> / <c>transform_back</c>（Animation Event <see cref="Chess.AnimCallback"/> + 超时兜底）。
/// </summary>
[Serializable]
public class PassiveSkillEffect_Hanekawa : ISkillEffect
{
    public const string BlackFormContextKey = "hanekawa_black";
    public const string AnimCallbackContextKey = "animCallback";

    [SerializeReference]
    [Tooltip("自身压力 Buff 模板")]
    public Buff_StressBuff_Death selfStressBuff;

    [Min(1)]
    [Tooltip("变身所需压力")]
    public int transformStress = 90;

    [Tooltip("黑形态每秒减少的压力")]
    public int stressDrainPerSecond = 2;

    [Min(0f)]
    [Tooltip("变身时 MaxHp 增加倍数：MaxHp += 当前MaxHp × 此值（默认 5 → +500%）")]
    public float hpMaxBonusMultiplier = 5f;

    [Min(0f)]
    [Tooltip("黑形态生命偷取增量")]
    public float lifeStealBonus = 0.3f;

    [Min(0.05f)]
    [Tooltip("索敌检测间隔（秒）")]
    public float seekInterval = 0.2f;

    [Tooltip("黑形态移速；≤0 用属性移速")]
    public float moveSpeed = 4f;

    [Tooltip("Animator Blend：常态")]
    public float normalBlend = 0f;

    [Tooltip("Animator Blend：黑形态")]
    public float blackBlend = 1f;

    [Tooltip("白→黑变身状态名")]
    public string transformStateName = "transform";

    [Tooltip("黑→白变身状态名")]
    public string transformBackStateName = "transform_back";

    [Min(0.1f)]
    [Tooltip("变身动画超时兜底（秒）；末帧未调 AnimCallback 时仍结束")]
    public float transformFallbackSeconds = 1.2f;

    [Min(0.1f)]
    [Tooltip("回白动画超时兜底（秒）")]
    public float transformBackFallbackSeconds = 1.2f;

    [Tooltip("黑形态大范围索敌（优先最近）；与武器近战索敌分开配置")]
    public IGridFindTarget seekGrid;

    Chess _user;
    Tile _homeTile;
    Timer _drainTimer;
    Timer _seekTimer;
    Timer _morphFallbackTimer;
    bool _black;
    bool _returning;
    bool _moving;
    bool _morphing;
    bool _pendingReturn;
    float _hpBonusApplied;
    float _lifeStealApplied;
    UnityAction _onStress;
    UnityAction<Chess> _onRemove;
    UnityAction _animCallback;
    readonly List<Chess> _seekTargets = new List<Chess>(32);

    enum MorphKind { None, ToBlack, ToWhite }
    MorphKind _morphKind;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;
        Cleanup();
        _user = user;
        _black = false;
        _returning = false;
        _moving = false;
        _morphing = false;
        _pendingReturn = false;
        _morphKind = MorphKind.None;
        _hpBonusApplied = 0f;
        _lifeStealApplied = 0f;
        _homeTile = null;

        EnsureDefaults();
        EnsureSelfStress(user);

        if (user.equipWeapon != null)
            user.equipWeapon.AttackAble = false;

        SetBlackFlag(false);
        ApplyBlend(normalBlend);

        _onStress = OnStressChanged;
        user.skillController?.context?.AddEvent(_onStress);

        _onRemove = OnRemoved;
        user.OnRemove.AddListener(_onRemove);

        OnStressChanged();
    }

    void EnsureDefaults()
    {
        if (seekGrid == null)
            seekGrid = new IGridFindTarget();
        if (seekGrid.relativeCells == null)
            seekGrid.relativeCells = new List<Vector2Int>();
        if (seekGrid.relativeCells.Count == 0)
        {
            for (int x = -1; x <= 8; x++)
            {
                for (int y = -2; y <= 2; y++)
                    seekGrid.relativeCells.Add(new Vector2Int(x, y));
            }
        }
        if (string.IsNullOrEmpty(transformStateName))
            transformStateName = "transform";
        if (string.IsNullOrEmpty(transformBackStateName))
            transformBackStateName = "transform_back";
    }

    void EnsureSelfStress(Chess user)
    {
        if (user.buffController == null) return;
        if (user.buffController.buffDic != null && user.buffController.buffDic.ContainsKey("压力"))
            return;

        Buff_StressBuff_Death buff = selfStressBuff != null
            ? (Buff_StressBuff_Death)selfStressBuff.Clone()
            : new Buff_StressBuff_Death { stressLimit = 999 };
        user.buffController.AddBuff(buff);
    }

    void OnStressChanged()
    {
        if (_user == null || _user.IfDeath) return;

        int stress = GetStress();
        if (_morphing && _morphKind == MorphKind.ToBlack && stress <= 0)
        {
            _pendingReturn = true;
            return;
        }

        if (!_black && !_returning && !_morphing && stress >= Mathf.Max(1, transformStress))
            EnterBlackForm();
        else if (_black && !_returning && !_morphing && stress <= 0)
            BeginReturnHome();
    }

    int GetStress()
    {
        int stress = 0;
        _user?.skillController?.context?.TryGet("stress", out stress);
        return stress;
    }

    void EnterBlackForm()
    {
        if (_user == null || _black || _morphing) return;

        _black = true;
        _returning = false;
        _pendingReturn = false;
        SetBlackFlag(true);
        // Blend 等变身动画结束后再切；变身 Clip 自身表现白→黑

        float maxHp = _user.propertyController.GetMaxHp();
        _hpBonusApplied = maxHp * Mathf.Max(0f, hpMaxBonusMultiplier);
        if (_hpBonusApplied > 0f)
            _user.propertyController.ChangeHPMax(_hpBonusApplied);

        _lifeStealApplied = Mathf.Max(0f, lifeStealBonus);
        if (_lifeStealApplied > 0f)
            _user.propertyController.ChangeLifeSteeling(_lifeStealApplied);

        ReserveHomeAndLeave();

        if (_user.equipWeapon != null)
            _user.equipWeapon.AttackAble = false;

        StartDrainTimer();
        StartMorph(MorphKind.ToBlack, transformStateName, transformFallbackSeconds, OnMorphToBlackDone);
    }

    void OnMorphToBlackDone()
    {
        if (_user == null || _user.IfDeath) return;
        ApplyBlend(blackBlend);
        StartSeekTimer();
        if (_pendingReturn || GetStress() <= 0)
            BeginReturnHome();
    }

    void ReserveHomeAndLeave()
    {
        Tile stand = _user.moveController?.standTile;
        if (stand == null) return;

        _homeTile = stand;
        stand.ChessLeave(_user);
        _homeTile.stander = _user;
    }

    void StartDrainTimer()
    {
        _drainTimer?.Stop();
        _drainTimer = null;
        if (GameManage.instance?.timerManage == null) return;
        _drainTimer = GameManage.instance.timerManage.AddTimer(DrainStressTick, 1f, true);
    }

    void DrainStressTick()
    {
        if (_user == null || _user.IfDeath || !_black || _returning) return;
        int drain = Mathf.Max(0, stressDrainPerSecond);
        if (drain <= 0) return;

        if (_user.buffController?.buffDic == null) return;
        if (!_user.buffController.buffDic.TryGetValue("压力", out Buff buff) || !(buff is Buff_StressBuff_Death stressBuff))
            return;

        stressBuff.BuffReset(new Buff_StressBuff_Death { extraStress = -drain });
    }

    void StartSeekTimer()
    {
        _seekTimer?.Stop();
        _seekTimer = null;
        if (GameManage.instance?.timerManage == null) return;
        float interval = Mathf.Max(0.05f, seekInterval);
        OnSeekPulse();
        _seekTimer = GameManage.instance.timerManage.AddTimer(OnSeekPulse, interval, true);
    }

    void OnSeekPulse()
    {
        if (_user == null || _user.IfDeath || !_black || _returning || _moving || _morphing)
            return;
        if (LevelManage.instance != null && !LevelManage.instance.IfGameStart)
            return;

        Chess target = FindNearestEnemy();
        if (target == null)
        {
            SetAttackAble(false);
            return;
        }

        if (IsInMeleeRange(target))
        {
            SetAttackAble(true);
            return;
        }

        if (!TryGetApproachTile(target, out Tile dest) || dest == null)
        {
            SetAttackAble(false);
            return;
        }

        if (_user.moveController.standTile == dest)
        {
            SetAttackAble(true);
            return;
        }

        SetAttackAble(false);
        _moving = true;

        float speed = moveSpeed > 0.01f
            ? moveSpeed
            : Mathf.Max(0.01f, _user.propertyController.GetMoveSpeed());

        FaceToward(dest);
        _user.moveController.MoveToTarget(dest, speed, OnMoveArrived);
    }

    void OnMoveArrived()
    {
        _moving = false;
        if (_user == null || _user.IfDeath) return;
        if (_returning)
        {
            FinishReturnHome();
            return;
        }
        OnSeekPulse();
    }

    Chess FindNearestEnemy()
    {
        _seekTargets.Clear();
        seekGrid?.FindTarget(_user, _seekTargets);
        Chess best = null;
        float bestDist = float.MaxValue;
        Vector3 origin = _user.transform.position;
        for (int i = 0; i < _seekTargets.Count; i++)
        {
            Chess c = _seekTargets[i];
            if (c == null || c.IfDeath) continue;
            float d = (c.transform.position - origin).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = c;
            }
        }
        return best;
    }

    bool IsInMeleeRange(Chess enemy)
    {
        if (_user?.equipWeapon?.weapon != null && _user.equipWeapon.weapon.FindEnemy(_user) > 0)
            return true;

        Tile a = _user.moveController?.standTile;
        Tile b = enemy.moveController?.standTile;
        if (a != null && b != null)
        {
            int dx = Mathf.Abs(a.mapPos.x - b.mapPos.x);
            int dy = Mathf.Abs(a.mapPos.y - b.mapPos.y);
            return dx + dy <= 1;
        }

        float tile = MapManage.instance != null ? MapManage.instance.tileSize.x : 1.5f;
        return Vector2.Distance(_user.transform.position, enemy.transform.position) <= tile * 1.35f;
    }

    bool TryGetApproachTile(Chess enemy, out Tile dest)
    {
        dest = null;
        var map = MapManage.instance;
        if (map == null || enemy == null) return false;

        Tile enemyTile = enemy.moveController?.standTile;
        if (enemyTile == null)
        {
            if (!TryWorldToTile(enemy.transform.position, out enemyTile) || enemyTile == null)
                return false;
        }

        Vector2Int[] offs =
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        float best = float.MaxValue;
        Vector3 from = _user.transform.position;
        for (int i = 0; i < offs.Length; i++)
        {
            int tx = enemyTile.mapPos.x + offs[i].x;
            int ty = enemyTile.mapPos.y + offs[i].y;
            if (tx < 0 || ty < 0 || tx >= map.mapSize.x || ty >= map.mapSize.y)
                continue;
            Tile t = map.tiles[tx, ty];
            if (t == null) continue;
            float d = (t.transform.position - from).sqrMagnitude;
            if (d < best)
            {
                best = d;
                dest = t;
            }
        }
        return dest != null;
    }

    bool TryWorldToTile(Vector3 world, out Tile tile)
    {
        tile = null;
        var map = MapManage.instance;
        if (map?.tiles == null) return false;
        float best = float.MaxValue;
        for (int x = 0; x < map.mapSize.x; x++)
        {
            for (int y = 0; y < map.mapSize.y; y++)
            {
                Tile t = map.tiles[x, y];
                if (t == null) continue;
                float d = (t.transform.position - world).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    tile = t;
                }
            }
        }
        return tile != null;
    }

    void BeginReturnHome()
    {
        if (_user == null || _returning) return;
        _returning = true;
        _moving = false;
        _pendingReturn = false;

        _seekTimer?.Stop();
        _seekTimer = null;
        _drainTimer?.Stop();
        _drainTimer = null;

        SetAttackAble(false);
        _user.equipWeapon?.StopAttack();
        _user.moveController?.StopMove();

        if (_homeTile == null)
        {
            FailReturnAndDie();
            return;
        }

        if (_homeTile.stander != null && _homeTile.stander != _user)
        {
            FailReturnAndDie();
            return;
        }

        if (_user.moveController.standTile == _homeTile)
        {
            FinishReturnHome();
            return;
        }

        _moving = true;
        float speed = moveSpeed > 0.01f
            ? moveSpeed
            : Mathf.Max(0.01f, _user.propertyController.GetMoveSpeed());
        FaceToward(_homeTile);
        _user.moveController.MoveToTarget(_homeTile, speed, OnMoveArrived);
    }

    void FinishReturnHome()
    {
        _moving = false;
        if (_user == null || _user.IfDeath) return;

        if (_homeTile == null ||
            (_homeTile.stander != null && _homeTile.stander != _user))
        {
            FailReturnAndDie();
            return;
        }

        if (_user.moveController.standTile != _homeTile ||
            _homeTile.chessesIntile == null ||
            !_homeTile.chessesIntile.Contains(_user))
        {
            _homeTile.ChessEnter(_user);
        }
        _homeTile.stander = _user;

        // 回白变身动画结束后再撤数值 / Blend
        StartMorph(MorphKind.ToWhite, transformBackStateName, transformBackFallbackSeconds, OnMorphToWhiteDone);
    }

    void OnMorphToWhiteDone()
    {
        if (_user == null || _user.IfDeath) return;

        RevertBlackStats();
        _black = false;
        _returning = false;
        SetBlackFlag(false);
        ApplyBlend(normalBlend);
        SetAttackAble(false);
        _homeTile = null;
    }

    void StartMorph(MorphKind kind, string stateName, float fallbackSeconds, UnityAction onDone)
    {
        StopMorphTimersOnly();
        _morphing = true;
        _morphKind = kind;
        SetAttackAble(false);

        _animCallback = () =>
        {
            if (!_morphing || _morphKind != kind) return;
            CompleteMorph(onDone);
        };

        if (_user.skillController?.context != null)
            _user.skillController.context.Set(AnimCallbackContextKey, _animCallback);

        var anim = _user.animatorController?.animator;
        if (anim != null && !string.IsNullOrEmpty(stateName))
            anim.Play(stateName, 0, 0f);
        else
        {
            // 无 Animator 时直接结束
            CompleteMorph(onDone);
            return;
        }

        float fallback = Mathf.Max(0.1f, fallbackSeconds);
        if (GameManage.instance?.timerManage != null)
            _morphFallbackTimer = GameManage.instance.timerManage.AddTimer(
                () =>
                {
                    if (!_morphing || _morphKind != kind) return;
                    CompleteMorph(onDone);
                },
                fallback,
                false);
    }

    void CompleteMorph(UnityAction onDone)
    {
        if (!_morphing) return;
        StopMorphTimersOnly();
        _morphing = false;
        _morphKind = MorphKind.None;
        ClearAnimCallback();
        onDone?.Invoke();
    }

    void StopMorphTimersOnly()
    {
        _morphFallbackTimer?.Stop();
        _morphFallbackTimer = null;
    }

    void ClearAnimCallback()
    {
        _animCallback = null;
        if (_user?.skillController?.context != null &&
            _user.skillController.context.TryGet(AnimCallbackContextKey, out UnityAction _))
            _user.skillController.context.Remove(AnimCallbackContextKey);
    }

    void FailReturnAndDie()
    {
        StopMorphTimersOnly();
        _morphing = false;
        _morphKind = MorphKind.None;
        ClearAnimCallback();
        ClearHomeReservation();
        RevertBlackStats();
        _black = false;
        _returning = false;
        SetBlackFlag(false);
        if (_user != null && !_user.IfDeath)
            _user.Death();
    }

    void RevertBlackStats()
    {
        if (_user?.propertyController == null) return;

        if (_lifeStealApplied > 0f)
        {
            _user.propertyController.ChangeLifeSteeling(-_lifeStealApplied);
            _lifeStealApplied = 0f;
        }

        if (_hpBonusApplied > 0f)
        {
            _user.propertyController.ChangeHPMax(-_hpBonusApplied);
            _hpBonusApplied = 0f;
            float hp = _user.propertyController.GetHp();
            float max = _user.propertyController.GetMaxHp();
            if (hp > max)
                _user.propertyController.ChangeHp(max);
        }
    }

    void ClearHomeReservation()
    {
        if (_homeTile != null && _homeTile.stander == _user)
            _homeTile.stander = null;
        _homeTile = null;
    }

    void SetAttackAble(bool able)
    {
        if (_user?.equipWeapon == null) return;
        if (_user.equipWeapon.AttackAble == able) return;
        _user.equipWeapon.AttackAble = able;
        if (!able)
            _user.equipWeapon.StopAttack();
    }

    void SetBlackFlag(bool black)
    {
        _user?.skillController?.context?.Set(BlackFormContextKey, black);
    }

    void ApplyBlend(float blend)
    {
        _user?.animatorController?.ChangeFloat(blend);
    }

    void FaceToward(Tile tile)
    {
        if (_user == null || tile == null) return;
        Vector2 delta = (Vector2)(tile.transform.position - _user.transform.position);
        _user.UpdateFacingFromHorizontalMove(delta);
    }

    void OnRemoved(Chess _)
    {
        Cleanup();
    }

    void Cleanup()
    {
        _drainTimer?.Stop();
        _drainTimer = null;
        _seekTimer?.Stop();
        _seekTimer = null;
        StopMorphTimersOnly();
        ClearAnimCallback();

        if (_user != null)
        {
            if (_onStress != null)
                _user.skillController?.context?.RemoveEvent(_onStress);
            if (_onRemove != null)
                _user.OnRemove.RemoveListener(_onRemove);
            _user.moveController?.StopMove();
            _user.equipWeapon?.StopAttack();
        }

        if (_black)
            RevertBlackStats();
        ClearHomeReservation();
        SetBlackFlag(false);

        _user = null;
        _black = false;
        _returning = false;
        _moving = false;
        _morphing = false;
        _pendingReturn = false;
        _morphKind = MorphKind.None;
        _onStress = null;
        _onRemove = null;
    }
}

/// <summary>
/// ColdSkill ready：黑羽川形态下禁止产阳技能。
/// </summary>
[Serializable]
public class SkillReady_HanekawaCanProduceSun : ISkillReady
{
    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user?.skillController?.context == null)
            return true;
        if (user.skillController.context.TryGet(PassiveSkillEffect_Hanekawa.BlackFormContextKey, out bool black) && black)
            return false;
        return true;
    }

    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets) { }
}
