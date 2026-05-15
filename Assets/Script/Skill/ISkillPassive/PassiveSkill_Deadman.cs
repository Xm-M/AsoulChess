using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 承受伤害统计：总伤为累计实际扣血量；DPS = 总伤 /（本轮首次受伤至当前这次受伤的 <see cref="TimerManage.GameTime"/> 间隔）。
/// 自最后一次受伤起 <see cref="idleResetSeconds"/> 内无新伤害则视为本轮结束（内部计数清空，<b>不</b>刷新 TMP）；同时将生命值回满。下次再受伤时从零开始新一轮并更新文字。
/// </summary>
public class PassiveSkill_Deadman : ISkillEffect
{
    [LabelText("总伤害 TMP")]
    public TMP_Text totalDamageText;

    [LabelText("DPS TMP")]
    public TMP_Text dpsText;

    [LabelText("无伤害多久后视为重置(秒)")]
    [MinValue(0.01f)]
    public float idleResetSeconds = 5f;

    const float MinDpsDt = 0.001f;

    Chess _user;
    Timer _idleTimer;

    bool _sessionAwaitingAfterIdle;
    bool _sessionActive;
    float _sessionTotal;
    float _sessionStartGameTime;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        _user = user;
        _sessionAwaitingAfterIdle = false;
        _sessionActive = false;
        _sessionTotal = 0f;

        user.propertyController.onGetDamage.AddListener(OnGetDamage);
        user.OnRemove.AddListener(OnChessRemove);
    }

    void OnGetDamage(DamageMessege dm)
    {
        if (_user == null || dm.damageTo != _user) return;

        float dmg = dm.damage;
        if (dmg <= 0f) return;

        float now = TimerManage.GameTime;

        if (_sessionAwaitingAfterIdle || !_sessionActive)
        {
            _sessionAwaitingAfterIdle = false;
            _sessionTotal = dmg;
            _sessionStartGameTime = now;
            _sessionActive = true;
        }
        else
            _sessionTotal += dmg;

        RefreshTexts(now);
        RestartIdleTimer();
    }

    void RefreshTexts(float now)
    {
        float dt = Mathf.Max(now - _sessionStartGameTime, MinDpsDt);
        float dps = _sessionTotal / dt;

        if (totalDamageText != null)
            totalDamageText.text = Mathf.RoundToInt(_sessionTotal).ToString();
        if (dpsText != null)
            dpsText.text = dps >= 100f ? dps.ToString("F0") : dps.ToString("F1");
    }

    void RestartIdleTimer()
    {
        _idleTimer?.Stop();
        _idleTimer = null;
        if (GameManage.instance?.timerManage != null)
            _idleTimer = GameManage.instance.timerManage.AddTimer(OnIdleSessionEnd, idleResetSeconds, false);
    }

    void OnIdleSessionEnd()
    {
        _idleTimer = null;
        if (!_sessionActive) return;
        _sessionActive = false;
        _sessionAwaitingAfterIdle = true;
        _sessionTotal = 0f;

        if (_user != null && !_user.IfDeath)
            _user.propertyController.ChangeHp(_user.propertyController.GetMaxHp());
    }

    void OnChessRemove(Chess chess)
    {
        if (_user != null)
        {
            _user.propertyController.onGetDamage.RemoveListener(OnGetDamage);
            _user.OnRemove.RemoveListener(OnChessRemove);
        }
        _idleTimer?.Stop();
        _idleTimer = null;
        _user = null;
    }
}
