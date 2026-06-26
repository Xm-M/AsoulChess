using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// M3 重构体被动：周期自损，生命不低于 <see cref="minHp"/>（避免 <see cref="IfDeathTransition"/> 触发死亡）。
/// </summary>
public class PassiveSkillEffect_M3RebuildDrain : ISkillEffect
{
    [Tooltip("自损 tick 间隔（秒）")]
    public float tickInterval = 1f;

    [Tooltip("每次 tick 扣除 MaxHp × 该比例")]
    public float hpLossPercentPerTick = 0.08f;

    [Tooltip("自损后保留的最低生命（>0）")]
    public float minHp = 1f;

    [Tooltip("不飘伤害数字")]
    public bool suppressFloatingDamage = true;

    Chess _user;
    Timer _timer;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null)
            return;

        _user = user;
        user.OnRemove.RemoveListener(OnRemove);
        user.OnRemove.AddListener(OnRemove);

        var tm = GameManage.instance?.timerManage;
        if (tm == null)
            return;

        ApplyTick();
        _timer = tm.AddTimer(ApplyTick, tickInterval, true);
    }

    void ApplyTick()
    {
        if (_user == null || _user.IfDeath)
            return;

        var pc = _user.propertyController;
        float hp = pc.GetHp();
        float maxHp = pc.GetMaxHp();
        if (maxHp <= 0f)
            return;

        float floor = Mathf.Max(0.0001f, minHp);
        if (hp <= floor)
            return;

        float loss = maxHp * hpLossPercentPerTick;
        float after = Mathf.Max(floor, hp - loss);
        float actualLoss = hp - after;
        if (actualLoss <= 0f)
            return;

        var dm = new DamageMessege(_user, _user, actualLoss, DamageType.Real);
        dm.suppressFloatingDamage = suppressFloatingDamage;
        pc.GetDamage(dm);

        if (pc.GetHp() < floor)
            pc.ChangeHp(floor);
    }

    void OnRemove(Chess chess)
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer = null;
        }

        if (_user != null)
            _user.OnRemove.RemoveListener(OnRemove);
        _user = null;
    }
}
