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
    Timer _chaseTimer;
    UnityAction<Chess> _onRemove;

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

        if (GameManage.instance?.timerManage != null)
            _chaseTimer = GameManage.instance.timerManage.AddTimer(ChaseTick, Mathf.Max(0.02f, chaseInterval), true);

        _onRemove = OnRemove;
        user.OnRemove.AddListener(_onRemove);
        ChaseTick();
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

        // 攻击中：不抢动画，只做轻量分离，避免围殴叠模
        if (sn == StateName.AttackState)
        {
            ApplySeparationOnly(speed, dt);
            return;
        }

        Chess enemy = FindTarget_NearestEnemyInRange.FindNearestEnemy(_user, out float dist);
        if (enemy == null)
        {
            ApplySeparationOnly(speed, dt);
            return;
        }

        float range = _user.propertyController != null
            ? _user.propertyController.GetAttackRange()
            : 1f;

        Vector3 pos = _user.transform.position;
        Vector3 sep = ComputeSeparation(pos);
        Vector3 delta;

        if (dist <= range)
        {
            // 已进距：停追，仅分离（围在目标周围而不是叠点）
            delta = sep * (speed * separationStrength * dt);
            if (delta.sqrMagnitude < 1e-8f)
                return;
            _user.transform.position = pos + delta;
            return;
        }

        Vector3 toEnemy = enemy.transform.position - pos;
        Vector3 seek = toEnemy.sqrMagnitude > 1e-6f
            ? toEnemy.normalized * (speed * dt)
            : Vector3.zero;
        delta = seek + sep * (speed * separationStrength * dt);

        // 限制本帧最大位移，避免分离过猛飞出
        float maxStep = speed * dt * (1f + separationStrength);
        if (delta.sqrMagnitude > maxStep * maxStep)
            delta = delta.normalized * maxStep;

        _user.UpdateFacingFromHorizontalMove((Vector2)toEnemy);
        _user.transform.position = pos + delta;
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
    }
}
