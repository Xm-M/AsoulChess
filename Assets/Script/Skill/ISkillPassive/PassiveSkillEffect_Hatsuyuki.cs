using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 初雪被动：每间隔 <see cref="placeIceIntervalSeconds"/> 用八向 BFS 铺冰（<see cref="Effect_Snow"/>）；
/// 冰持续 <see cref="iceDurationSeconds"/>（应大于间隔）。进场后 <see cref="AttackController.AttackAble"/> 为 false；
/// 主动技能里挂 <see cref="SkillEffect_HatsuyukiUnlockAttack"/>：施放一次后永久解锁普攻（<see cref="AttackController.AttackAble"/> 置 true，之后不再关回）。
/// 初雪冰对敌对阵营造成踩入伤害：<see cref="iceEnterDamageCoeff"/> × 攻击力。
/// </summary>
[Serializable]
public class PassiveSkillEffect_Hatsuyuki : ISkillEffect
{
    [Min(0.1f)]
    public float placeIceIntervalSeconds = 6f;

    [Min(0.1f)]
    public float iceDurationSeconds = 15f;

    [Min(0f)]
    [Tooltip("踩入初雪冰格伤害 = 初雪攻击力 × 本系数")]
    public float iceEnterDamageCoeff = 0.3f;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;
        var r = user.GetComponent<HatsuyukiPassiveRuntime>();
        if (r == null)
            r = user.gameObject.AddComponent<HatsuyukiPassiveRuntime>();
        r.Setup(user, this);
    }
}

/// <summary>初雪被动运行时：定时铺冰；进场 <see cref="AttackController.AttackAble"/> 为 false，用主动技能解锁后保持可普攻。</summary>
public class HatsuyukiPassiveRuntime : MonoBehaviour
{
    Chess _user;
    PassiveSkillEffect_Hatsuyuki _cfg;
    Timer _iceTimer;

    public void Setup(Chess user, PassiveSkillEffect_Hatsuyuki cfg)
    {
        if (_user != null)
            Unhook();

        _user = user;
        _cfg = cfg;
        if (_user == null || _cfg == null)
            return;

        if (_user.equipWeapon != null)
            _user.equipWeapon.AttackAble = false;

        _user.OnRemove.AddListener(OnChessRemove);

        Effect_Snow.GetInstanceOrNull()?.RegisterHatsuyuki(_user, _cfg.iceEnterDamageCoeff);

        float interval = _cfg.placeIceIntervalSeconds;
        if (interval > 0f && GameManage.instance != null && GameManage.instance.timerManage != null)
            _iceTimer = GameManage.instance.timerManage.AddTimer(OnIceTick, interval, true);
    }

    void OnChessRemove(Chess c)
    {
        Unhook();
    }

    void OnIceTick()
    {
        if (_user == null || MapManage.instance == null || _cfg == null)
            return;

        var snow = Effect_Snow.GetInstanceOrNull();
        if (snow == null) return;

        Vector2Int start = GetBaseMapPos(_user);
        snow.TryBfsPlaceFirstEmptyIce(start, _cfg.iceDurationSeconds, _user.tag, _user);
    }

    static Vector2Int GetBaseMapPos(Chess user)
    {
        if (user.moveController != null && user.moveController.standTile != null)
            return user.moveController.standTile.mapPos;

        var map = MapManage.instance;
        Vector2 p = user.transform.position;
        Vector2 ts = map.tileSize;
        int ix = Mathf.FloorToInt(p.x / ts.x);
        int iy = Mathf.FloorToInt(p.y / ts.y);
        ix = Mathf.Clamp(ix, 0, Mathf.Max(0, map.mapSize.x - 1));
        iy = Mathf.Clamp(iy, 0, Mathf.Max(0, map.mapSize.y - 1));
        return new Vector2Int(ix, iy);
    }

    void Unhook()
    {
        if (_iceTimer != null)
        {
            _iceTimer.Stop();
            _iceTimer = null;
        }

        if (_user != null)
        {
            _user.OnRemove.RemoveListener(OnChessRemove);
            Effect_Snow.GetInstanceOrNull()?.UnregisterHatsuyuki(_user);
        }

        _user = null;
        _cfg = null;
    }

    void OnDisable()
    {
        Unhook();
    }
}
