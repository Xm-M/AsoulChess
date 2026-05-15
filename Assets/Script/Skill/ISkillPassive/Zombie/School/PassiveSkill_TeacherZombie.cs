using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

/// <summary>
/// 教师僵尸被动：每隔 <see cref="interval"/> 秒，在半径 <see cref="radius"/> 内寻找<strong>同阵营</strong>、且位于自身 <see cref="Chess.transform.right"/> 一侧（点积 &gt; 0）的单位，为其添加 <see cref="Buff_ZombieTeacher"/>。
/// </summary>
public class PassiveSkill_TeacherZombie : ISkillEffect
{
    [LabelText("检测间隔(秒)")]
    [MinValue(0.1f)]
    public float interval = 3f;

    [LabelText("半径")]
    [MinValue(0.1f)]
    public float radius = 5f;

    [SerializeReference, LabelText("教师庇护 Buff")]
    public Buff_ZombieTeacher teacherBuff;

    Chess _user;
    Timer _timer;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        _user = user;
        if (teacherBuff == null)
        {
            Debug.LogWarning("[PassiveSkill_TeacherZombie] 未配置 Buff_ZombieTeacher");
            return;
        }
        if (GameManage.instance?.timerManage == null) return;

        _timer = GameManage.instance.timerManage.AddTimer(OnTick, interval, true);
        user.OnRemove.AddListener(OnChessRemove);
    }

    void OnTick()
    {
        if (_user == null || _user.IfDeath || ChessTeamManage.Instance == null) return;

        var team = ChessTeamManage.Instance.GetTeam(_user.tag);
        if (team == null) return;

        Vector2 origin = _user.transform.position;
        Vector2 side = _user.transform.right;

        float r2 = radius * radius;
        for (int i = 0; i < team.Count; i++)
        {
            var c = team[i];
            if (c == null || c == _user || c.IfDeath) continue;

            Vector2 delta = (Vector2)c.transform.position - origin;
            if (delta.sqrMagnitude > r2) continue;
            if (Vector2.Dot(delta, side) <= 0.001f) continue;

            c.buffController.AddBuff(teacherBuff);
        }
    }

    void OnChessRemove(Chess chess)
    {
        _timer?.Stop();
        _timer = null;
        if (_user != null)
            _user.OnRemove.RemoveListener(OnChessRemove);
        _user = null;
    }
}

/// <summary>
/// 教师庇护：单次受到的伤害（在护甲等结算之前，见 <see cref="PropertyController.onSetDamage"/>）不会超过自身最大生命值的 <see cref="maxHpDamageFraction"/>。
/// </summary>
public class Buff_ZombieTeacher : Buff
{
    [LabelText("单次伤害上限(相对最大生命比例)")]
    [Range(0.01f, 1f)]
    public float maxHpDamageFraction = 0.2f;

    UnityAction<DamageMessege> _onSetDamage;

    public Buff_ZombieTeacher()
    {
        buffName = "教师庇护";
    }

    public override Buff Clone() => (Buff_ZombieTeacher)MemberwiseClone();

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        _onSetDamage = ClampIncomingDamage;
        target.propertyController.onSetDamage.AddListener(_onSetDamage);
    }

    void ClampIncomingDamage(DamageMessege mes)
    {
        if (target == null || mes.damageTo != target) return;
        if (mes.damageType == DamageType.Heal || mes.damageType == DamageType.Miss)
            return;
        if (mes.damage <= 0f) return;

        float cap = target.propertyController.GetMaxHp() * maxHpDamageFraction;
        if (mes.damage > cap)
        {
            mes.damage = cap;
            UIManage.GetView<DamagePanel>().ShowText(mes, "双方都有错", Color.black);
        }
    }

    public override void BuffOver()
    {
        if (target != null && _onSetDamage != null)
            target.propertyController.onSetDamage.RemoveListener(_onSetDamage);
        _onSetDamage = null;
        base.BuffOver();
    }
}
