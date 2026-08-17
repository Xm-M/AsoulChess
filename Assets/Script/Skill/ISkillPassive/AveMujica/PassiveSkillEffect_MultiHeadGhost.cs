using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 多首幽灵：不占格召唤物被动 — UnSelectable、关碰撞、世界坐标追击最近敌人。
/// 同队幽灵之间用软分离推开，避免追击/围殴时叠在一起（不依赖 Rigidbody2D）。
/// </summary>
[Serializable]
public class PassiveSkillEffect_MultiHeadGhost : ISkillEffect
{
    [Min(0.02f)]
    [Tooltip("追击刷新间隔（秒）")]
    public float chaseInterval = 0.05f;

    [Min(0.05f)]
    [Tooltip("幽灵彼此理想间距；小于此距离会互相推开")]
    public float separationRadius = 0.75f;

    [Min(0f)]
    [Tooltip("分离推力相对移速的倍率")]
    public float separationStrength = 1.4f;

    Chess _user;
    Chess _master;
    Chess _followTarget;
    Vector3 _lastFollowTargetPos;
    bool _hasLastFollowPos;
    Timer _chaseTimer;
    UnityAction<Chess> _onRemove;
    UnityAction<Chess> _onAttack;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;
        _user = user;
        user.skillController?.context?.TryGet(MultiHeadMutsumiKeys.GhostMaster, out _master);

        user.UnSelectable();
        user.SetCol(false);
        if (user.moveController != null)
        {
            user.moveController.standTile = null;
            user.moveController.tileMethod = null;
        }

        if (user.equipWeapon != null)
        {
            _onAttack = OnGhostAttack;
            user.equipWeapon.OnAttack.AddListener(_onAttack);
        }

        if (GameManage.instance?.timerManage != null)
            _chaseTimer = GameManage.instance.timerManage.AddTimer(ChaseTick, Mathf.Max(0.02f, chaseInterval), true);

        _onRemove = OnRemove;
        user.OnRemove.AddListener(_onRemove);
        ChaseTick();
    }

    /// <summary>普攻结算后追加：数值 = 主人当前压力的真实伤害；压力 ≤0 不追加。</summary>
    void OnGhostAttack(Chess attacker)
    {
        if (attacker != _user || _master == null || _master.IfDeath)
            return;

        int stress = MultiHeadMutsumiKeys.GetCurrentStress(_master);
        if (stress <= 0)
            return;

        if (attacker.equipWeapon?.weapon is not Weapon_Sample weapon || weapon.enemys == null)
            return;

        ElementType element = ElementType.CloseAttack;
        if (weapon.attackFunction is CloseAttack closeAttack && closeAttack.DM != null)
            element = closeAttack.DM.damageElementType;

        for (int i = 0; i < weapon.enemys.Count; i++)
        {
            Chess target = weapon.enemys[i];
            if (target == null || target.IfDeath)
                continue;

            var bonus = new DamageMessege(
                attacker,
                target,
                stress,
                DamageType.Real,
                element);
            attacker.propertyController.TakeDamage(bonus);
        }
    }

    void ChaseTick()
    {
        if (_user == null || _user.IfDeath || !LevelManage.instance.IfGameStart)
            return;

        StateName sn = _user.stateController?.currentState?.state?.stateName ?? StateName.IdleState;

        // 进场 Resume：只等 resume 播完，禁止位移/切 run（否则永远退不出 ResumeState）
        if (sn == StateName.ResumeState || sn == StateName.DeathState)
            return;

        // Resume.Exit 会 ResumeSelectable；幽灵须保持不可选中与无碰撞
        EnsureGhostInvulnerable();

        float dt = Mathf.Max(0.02f, chaseInterval);
        float speed = _user.propertyController != null
            ? Mathf.Max(0.1f, _user.propertyController.GetMoveSpeed())
            : 2f;

        float range = _user.propertyController != null
            ? _user.propertyController.GetAttackRange()
            : 1f;

        Chess enemy = ResolveFollowTarget(sn, out float dist);
        if (enemy == null)
        {
            ClearFollowTarget();
            ApplySeparationOnly(speed, dt);
            return;
        }

        SyncFollowTarget(enemy);

        // 进距或攻击中：随目标位移并保持在近战范围内，避免站桩脱战
        if (sn == StateName.AttackState || dist <= range)
        {
            ApplyFollowEngaged(enemy, range, speed, dt, sn);
            return;
        }

        Vector3 pos = _user.transform.position;
        Vector3 sep = ComputeSeparation(pos);
        Vector3 toEnemy = enemy.transform.position - pos;
        Vector3 seek = toEnemy.sqrMagnitude > 1e-6f
            ? toEnemy.normalized * (speed * dt)
            : Vector3.zero;
        Vector3 delta = seek + sep * (speed * separationStrength * dt);

        float maxStep = speed * dt * (1f + separationStrength);
        if (delta.sqrMagnitude > maxStep * maxStep)
            delta = delta.normalized * maxStep;

        _user.UpdateFacingFromHorizontalMove((Vector2)toEnemy);
        _user.transform.position = pos + delta;
        EnsurePlayRun();
    }

    /// <summary>攻击态优先锁 weapon 目标，否则最近敌人。</summary>
    Chess ResolveFollowTarget(StateName sn, out float dist)
    {
        dist = float.MaxValue;
        if (_user == null)
            return null;

        if (sn == StateName.AttackState
            && _user.equipWeapon?.weapon is Weapon_Sample weapon
            && weapon.enemys != null)
        {
            for (int i = 0; i < weapon.enemys.Count; i++)
            {
                Chess locked = weapon.enemys[i];
                if (locked == null || locked.IfDeath)
                    continue;
                dist = Vector2.Distance(_user.transform.position, locked.transform.position);
                return locked;
            }
        }

        return FindTarget_NearestEnemyInRange.FindNearestEnemy(_user, out dist);
    }

    void SyncFollowTarget(Chess enemy)
    {
        if (_followTarget == enemy)
            return;
        _followTarget = enemy;
        _hasLastFollowPos = false;
    }

    void ClearFollowTarget()
    {
        _followTarget = null;
        _hasLastFollowPos = false;
    }

    /// <summary>贴随目标：叠加目标本帧位移 + 超出栓绳距离时补追击；攻击态不切 run 动画。</summary>
    void ApplyFollowEngaged(Chess enemy, float range, float speed, float dt, StateName sn)
    {
        Vector3 pos = _user.transform.position;
        Vector3 enemyPos = enemy.transform.position;
        Vector3 sep = ComputeSeparation(pos);
        Vector3 delta = Vector3.zero;

        if (_hasLastFollowPos)
            delta += enemyPos - _lastFollowTargetPos;

        Vector3 toEnemy = enemyPos - pos;
        float dist = toEnemy.magnitude;
        float leash = range * 0.92f;
        if (dist > leash && toEnemy.sqrMagnitude > 1e-6f)
            delta += toEnemy.normalized * Mathf.Min(dist - leash, speed * dt);

        delta += sep * (speed * separationStrength * dt);

        if (delta.sqrMagnitude > 1e-8f)
        {
            if (Mathf.Abs(toEnemy.x) > 0.01f)
                _user.UpdateFacingFromHorizontalMove((Vector2)toEnemy);
            _user.transform.position = pos + delta;
        }

        _lastFollowTargetPos = enemyPos;
        _hasLastFollowPos = true;

        if (sn != StateName.AttackState)
            EnsurePlayRun();
    }

    void ApplySeparationOnly(float speed, float dt)
    {
        Vector3 pos = _user.transform.position;
        Vector3 sep = ComputeSeparation(pos);
        Vector3 delta = sep * (speed * separationStrength * dt);
        if (delta.sqrMagnitude < 1e-8f)
            return;
        _user.transform.position = pos + delta;
    }

    /// <summary>相对同队幽灵的单位分离向量（已按距离衰减，模长约 0~同邻数）。</summary>
    Vector3 ComputeSeparation(Vector3 myPos)
    {
        if (_master == null
            || !MultiHeadMutsumiKeys.TryGetGhostList(_master, out List<Chess> list)
            || list == null
            || list.Count <= 1)
            return Vector3.zero;

        float radius = Mathf.Max(0.05f, separationRadius);
        Vector3 push = Vector3.zero;
        int contributors = 0;

        for (int i = 0; i < list.Count; i++)
        {
            Chess other = list[i];
            if (other == null || other == _user || other.IfDeath)
                continue;

            Vector3 away = myPos - other.transform.position;
            float dist = away.magnitude;
            if (dist >= radius || dist < 1e-4f)
            {
                // 几乎重合：用稳定伪随机方向推开，避免 NaN
                if (dist < 1e-4f)
                {
                    float angle = (other.GetInstanceID() ^ _user.GetInstanceID()) * 0.001f;
                    away = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                    push += away;
                    contributors++;
                }
                continue;
            }

            float weight = 1f - dist / radius;
            push += (away / dist) * weight;
            contributors++;
        }

        if (contributors == 0 || push.sqrMagnitude < 1e-8f)
            return Vector3.zero;
        return push;
    }

    void EnsureGhostInvulnerable()
    {
        // IfSelectable==true 表示已在 Unselectable 层
        if (!_user.IfSelectable)
            _user.UnSelectable();
        _user.SetCol(false);
        if (_user.moveController != null)
        {
            _user.moveController.standTile = null;
            _user.moveController.tileMethod = null;
        }
    }

    /// <summary>仅在当前不是 run 时播一次，避免 Timer 每 tick Play 打断循环。</summary>
    void EnsurePlayRun()
    {
        Animator anim = _user?.animatorController?.animator;
        if (anim == null)
            return;
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("run"))
            return;
        _user.animatorController.PlayMove(); // → animator.Play("run")
    }

    void OnRemove(Chess chess)
    {
        if (_chaseTimer != null)
        {
            _chaseTimer.Stop();
            _chaseTimer = null;
        }

        if (_user?.equipWeapon != null && _onAttack != null)
            _user.equipWeapon.OnAttack.RemoveListener(_onAttack);

        // 从主人幽灵表摘除（用字段 _master：自身 context 在 OnRemove 前已 Clear）
        if (_master != null
            && MultiHeadMutsumiKeys.TryGetGhostList(_master, out List<Chess> list))
        {
            list.Remove(_user);
        }

        if (_user != null && _onRemove != null)
            _user.OnRemove.RemoveListener(_onRemove);
        _user = null;
        _master = null;
        _onRemove = null;
        _onAttack = null;
    }
}
