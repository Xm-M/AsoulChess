using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 田井中律被动：周期性以自身 <see cref="PropertyController.GetExtraDamage"/> × 系数
/// 为周围 3×3（不含自身格）友方刷新 <see cref="Buff_TainakaRitsuDrumHealAura"/>（同名 BuffReset，不叠加）。
/// </summary>
public class PassiveSkillEffect_TainakaRitsu : ISkillEffect
{
    [LabelText("刷新间隔(秒)")] public float interval = 0.25f;

    [LabelText("增伤→治疗倍率"), Tooltip("友方获得的 extraHealRate = 律的 GetExtraDamage × 该系数")]
    [MinValue(0f)]
    public float healRateFromExtraDamageScale = 1f;

    Timer _timer;
    Chess _user;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        _user = user;

        RefreshAura();
        if (GameManage.instance != null && GameManage.instance.timerManage != null)
            _timer = GameManage.instance.timerManage.AddTimer(OnTick, interval, true);
        user.OnRemove.AddListener(OnChessRemove);
    }

    void OnTick()
    {
        RefreshAura();
    }

    void RefreshAura()
    {
        if (_user == null || _user.IfDeath || _user.moveController?.standTile == null) return;

        var map = MapManage.instance;
        if (map == null) return;

        float bonus = Mathf.Max(0f, _user.propertyController.GetExtraDamage() * healRateFromExtraDamageScale);
        var stand = _user.moveController.standTile;
        int cx = stand.mapPos.x;
        int cy = stand.mapPos.y;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int x = cx + dx;
                int y = cy + dy;
                if (!map.IfInMapRange(x, y)) continue;
                var tile = map.tiles[x, y];
                var ally = tile?.stander;
                if (ally == null || ally.IfDeath || ally == _user) continue;
                if (!ally.CompareTag(_user.tag)) continue;

                var buff = new Buff_TainakaRitsuDrumHealAura
                {
                    buffName = Buff_TainakaRitsuDrumHealAura.DefaultBuffName,
                    extraHealRate = bonus
                };
                ally.buffController.AddBuff(buff);
            }
        }
    }

    /// <summary>棋子移除时停表并卸下 OnRemove（离场销毁也会触发）。</summary>
    void OnChessRemove(Chess c)
    {
        _timer?.Stop();
        _timer = null;
        if (_user != null)
            _user.OnRemove.RemoveListener(OnChessRemove);
        _user = null;
    }
}
