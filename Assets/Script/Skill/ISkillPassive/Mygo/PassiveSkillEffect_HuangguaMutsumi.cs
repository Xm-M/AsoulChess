using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 黄瓜睦（窝瓜）：同排左右各 N 格索敌 → 水平移到目标格 → 播 skill →
/// 动画事件 <see cref="Chess.UseSkill"/> 触发圆形 ATK 伤害后自毁。
/// </summary>
[Serializable]
public class PassiveSkillEffect_HuangguaMutsumi : ISkillEffect
{
    [Tooltip("索敌检测间隔（秒）")]
    public float checkInterval = 0.15f;

    [Min(1)]
    [Tooltip("同排左右各检测几格（1=仅邻格，2=左右各 2 格，含 ±1…±N）")]
    public int seekCellsEachSide = 2;

    [Tooltip("下砸圆形伤害半径（世界单位）")]
    public float smashRadius = 1.35f;

    [Tooltip("格心补检半径相对 smashRadius 的倍率（僵尸在格间时用）")]
    [Range(0.1f, 1f)]
    public float tileProbeRadiusScale = 0.55f;

    [Tooltip("水平移动速度；≤0 时用属性移速")]
    public float moveSpeed = 8f;

    [Tooltip("技能动画状态名（默认 skill）")]
    public string skillStateName = "skill";

    Chess _user;
    Timer _timer;
    bool _committed;
    bool _smashed;
    UnityAction<Chess> _onUseSkill;
    UnityAction<Chess> _onRemove;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null)
            return;

        _user = user;
        _committed = false;
        _smashed = false;

        if (user.equipWeapon != null)
            user.equipWeapon.AttackAble = false;

        _onUseSkill = OnAnimUseSkill;
        _onRemove = OnRemoved;
        user.skillController.onUseSkill.AddListener(_onUseSkill);
        user.OnRemove.AddListener(_onRemove);

        if (GameManage.instance?.timerManage == null)
            return;

        float interval = Mathf.Max(0.05f, checkInterval);
        _timer = GameManage.instance.timerManage.AddTimer(TrySeekAndPounce, interval, true);
    }

    void OnRemoved(Chess _)
    {
        Cleanup();
    }

    void Cleanup()
    {
        _timer?.Stop();
        _timer = null;
        if (_user == null)
            return;
        if (_onUseSkill != null)
            _user.skillController?.onUseSkill.RemoveListener(_onUseSkill);
        if (_onRemove != null)
            _user.OnRemove.RemoveListener(_onRemove);
        _onUseSkill = null;
        _onRemove = null;
    }

    void TrySeekAndPounce()
    {
        if (_committed || _user == null || _user.IfDeath)
            return;
        if (LevelManage.instance != null && !LevelManage.instance.IfGameStart)
            return;

        if (!TryFindBestTargetTile(out Tile targetTile))
            return;

        _committed = true;
        _timer?.Stop();
        _timer = null;

        var stand = _user.moveController?.standTile;
        if (stand != null)
            stand.ChessLeave(_user);

        _user.UnSelectable();
        FaceToward(targetTile);

        float speed = moveSpeed > 0.01f
            ? moveSpeed
            : Mathf.Max(0.01f, _user.propertyController.GetMoveSpeed());

        _user.moveController.MoveToTarget(targetTile, speed, OnArrivedAtTarget);
    }

    void FaceToward(Tile targetTile)
    {
        if (_user == null || targetTile == null)
            return;
        Vector2 delta = (Vector2)(targetTile.transform.position - _user.transform.position);
        _user.UpdateFacingFromHorizontalMove(delta);
    }

    void OnArrivedAtTarget()
    {
        if (_user == null || _user.IfDeath)
            return;

        // 进入 SkillState 会再调一次 PlaySkill；无 activeSkill 时 技能模版 的 IfAnimPlayOver 会 NRE，
        // 因此只播动画，不切状态机。
        if (_user.animatorController?.animator == null)
        {
            Debug.LogWarning("[黄瓜睦] animator 未绑定，无法播放 skill");
            ApplySmashDamage();
            _user.Death();
            return;
        }

        if (!string.IsNullOrEmpty(skillStateName) && skillStateName != "skill")
            _user.animatorController.animator.Play(skillStateName, 0, 0f);
        else
            _user.animatorController.PlaySkill();
    }

    void OnAnimUseSkill(Chess chess)
    {
        if (_smashed || chess != _user || _user == null || _user.IfDeath)
            return;
        if (!_committed)
            return;

        // 仅结算下砸伤害；自毁交给动画末尾 Death 事件（或超时兜底）
        _smashed = true;
        ApplySmashDamage();
        EnsureDeathFallback();
    }

    void EnsureDeathFallback()
    {
        if (_user == null || GameManage.instance?.timerManage == null)
            return;
        // 若动画未绑 Death，约 1 秒后仍自毁，避免卡住
        GameManage.instance.timerManage.AddTimer(() =>
        {
            if (_user != null && !_user.IfDeath)
                _user.Death();
        }, 1.2f, false);
    }

    void ApplySmashDamage()
    {
        float damage = _user.propertyController.GetAttack();
        if (damage <= 0f)
            return;

        Vector2 center = _user.transform.position;
        var col = _user.GetComponent<Collider2D>();
        if (col != null)
            center = col.bounds.center;

        var enemies = new List<Chess>(16);
        WisadelGridHelper.CollectEnemiesInCircle(_user, center, smashRadius, enemies);
        for (int i = 0; i < enemies.Count; i++)
        {
            Chess enemy = enemies[i];
            if (enemy == null || enemy.IfDeath)
                continue;
            var dm = _user.skillController.DM;
            dm.damageFrom = _user;
            dm.damageTo = enemy;
            dm.damage = damage;
            _user.propertyController.TakeDamage(dm);
        }
    }

    /// <summary>同排 ±1…±seekCellsEachSide；优先 |dx| 更近，同距取更靠家（x 更小）。</summary>
    bool TryFindBestTargetTile(out Tile best)
    {
        best = null;
        Tile origin = _user.moveController?.standTile;
        var map = MapManage.instance;
        if (origin == null || map == null)
            return false;

        int maxOffset = Mathf.Max(1, seekCellsEachSide);
        int bestAbsDx = int.MaxValue;
        int bestX = int.MaxValue;

        for (int dx = -maxOffset; dx <= maxOffset; dx++)
        {
            if (dx == 0)
                continue;

            int tx = origin.mapPos.x + dx;
            int ty = origin.mapPos.y;
            if (tx < 0 || tx >= map.mapSize.x || ty < 0 || ty >= map.mapSize.y)
                continue;

            Tile tile = map.tiles[tx, ty];
            if (tile == null || !TileHasEnemy(tile))
                continue;

            int absDx = Mathf.Abs(dx);
            if (absDx < bestAbsDx || (absDx == bestAbsDx && tx < bestX))
            {
                bestAbsDx = absDx;
                bestX = tx;
                best = tile;
            }
        }

        return best != null;
    }

    bool TileHasEnemy(Tile tile)
    {
        if (tile == null || _user == null)
            return false;

        if (tile.chessesIntile != null)
        {
            string enemyTag = _user.CompareTag("Player") ? "Enemy" : "Player";
            for (int i = 0; i < tile.chessesIntile.Count; i++)
            {
                Chess c = tile.chessesIntile[i];
                if (c == null || c.IfDeath || c == _user)
                    continue;
                if (c.CompareTag(enemyTag))
                    return true;
            }
        }

        float probeR = smashRadius * Mathf.Clamp(tileProbeRadiusScale, 0.1f, 1f);
        var enemies = new List<Chess>(8);
        WisadelGridHelper.CollectEnemiesInCircle(_user, tile.transform.position, probeR, enemies);
        return enemies.Count > 0;
    }
}
