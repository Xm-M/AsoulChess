using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 天素罗被动：入场后按关卡时间倒计时，到期自灭并在同格生成完整「长崎素世」。
/// 提前死亡/铲除不转化。无武器普攻（关闭 AttackAble）。
/// </summary>
[Serializable]
public class PassiveSkillEffect_TianSuoLuo : ISkillEffect
{
    [Tooltip("到期后在同格生成的完整长崎素世 PropertyCreator")]
    public PropertyCreator soyorinCreator;

    [Min(0.1f)]
    [Tooltip("种植后等待秒数（TimerManage 关卡时间，受暂停影响）")]
    public float delaySeconds = 180f;

    Chess _user;
    Tile _tile;
    Timer _timer;
    bool _shouldTransform;
    UnityAction<Chess> _onRemove;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null)
            return;

        _shouldTransform = false;
        _user = user;
        _tile = user.moveController != null ? user.moveController.standTile : null;

        if (user.equipWeapon != null)
            user.equipWeapon.AttackAble = false;

        _onRemove = OnUserRemove;
        user.OnRemove.AddListener(_onRemove);

        StopTimer();
        if (GameManage.instance?.timerManage == null)
            return;

        float delay = Mathf.Max(0.1f, delaySeconds);
        _timer = GameManage.instance.timerManage.AddTimer(OnMature, delay, false);
    }

    void OnMature()
    {
        if (_user == null || _user.IfDeath)
            return;

        // 刷新格子缓存（防挪位插件）；Death 后 standTile 会清空
        if (_user.moveController?.standTile != null)
            _tile = _user.moveController.standTile;

        _shouldTransform = true;
        _user.Death();
    }

    void OnUserRemove(Chess chess)
    {
        StopTimer();

        if (!_shouldTransform || soyorinCreator == null)
            return;

        _shouldTransform = false;

        Tile tile = _tile;
        if (tile == null)
            return;

        string teamTag = chess != null ? chess.tag : "Player";
        Chess spawned = GameManage.instance.chessTeamManage.CreateChess(soyorinCreator, tile, teamTag);
        if (spawned == null)
            return;

        // CreateChess → WhenChessEnterWar 已按 Prefab EnterWarState（素世为 Idle）入场；
        // 勿切 ResumeState：植物 MyGO 状态图通常无该键（气球僵尸落地专用）。
        spawned.transform.position = tile.transform.position;
    }

    void StopTimer()
    {
        if (_timer == null)
            return;
        _timer.Stop();
        _timer = null;
    }
}
