using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 攀附者被动：场上有霸凌者时随机跟随其一所在行（仅纵向对齐，保持当前列），
/// 并施加 <see cref="Buff_Zombie_ClingerAttach"/>（+100% 攻速）；无霸凌者时移除 Buff。
/// </summary>
public class PassiveSkill_Clinger : ISkillEffect
{
    public const string BullyChessName = "霸凌者";
    public const string AttachBuffName = "攀附";

    [LabelText("检测间隔(秒)")]
    [MinValue(0.1f)]
    public float interval = 0.3f;

    [SerializeReference, LabelText("攀附 Buff 模板")]
    public Buff_Zombie_ClingerAttach attachBuff;

    Chess _user;
    Timer _timer;
    bool _aligningRow;
    int _targetRowY = int.MinValue;
    Chess _followBully;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        _user = user;
        if (attachBuff == null)
            attachBuff = new Buff_Zombie_ClingerAttach();
        attachBuff.buffName = AttachBuffName;

        if (GameManage.instance?.timerManage == null) return;

        _timer = GameManage.instance.timerManage.AddTimer(OnTick, interval, true);
        user.OnRemove.AddListener(OnChessRemove);
        OnTick();
    }

    void OnTick()
    {
        if (_user == null || _user.IfDeath || ChessTeamManage.Instance == null) return;

        var bullies = CollectBullies();
        if (bullies.Count == 0)
        {
            _aligningRow = false;
            _targetRowY = int.MinValue;
            _followBully = null;
            RemoveAttachBuff();
            return;
        }

        EnsureAttachBuff();

        if (_followBully == null || _followBully.IfDeath || !bullies.Contains(_followBully))
            _followBully = bullies[Random.Range(0, bullies.Count)];

        var bullyTile = _followBully.moveController?.standTile;
        if (bullyTile == null) return;

        TryAlignRow(bullyTile.mapPos.y);
    }

    static bool IsOnRow(Chess chess, int targetY)
    {
        var stand = chess?.moveController?.standTile;
        if (stand != null && stand.mapPos.y == targetY) return true;
        var map = MapManage.instance;
        if (stand == null || map == null || !map.IfInMapRange(stand.mapPos.x, targetY)) return false;
        var rowTile = map.tiles[stand.mapPos.x, targetY];
        if (rowTile == null) return false;
        return Mathf.Abs(chess.transform.position.y - rowTile.transform.position.y) < 0.15f;
    }

    void RefreshHorMovePath()
    {
        var mc = _user?.moveController;
        if (mc?.tileMethod == null) return;
        mc.nextTile = mc.tileMethod.FindNextTile(_user);
    }

    List<Chess> CollectBullies()
    {
        var list = new List<Chess>();
        var team = ChessTeamManage.Instance.GetTeam(_user.tag);
        if (team == null) return list;

        for (int i = 0; i < team.Count; i++)
        {
            var c = team[i];
            if (c == null || c.IfDeath || c == _user) continue;
            if (c.propertyController?.creator?.chessName == BullyChessName)
                list.Add(c);
        }
        return list;
    }

    void EnsureAttachBuff()
    {
        if (_user.buffController.buffDic.ContainsKey(AttachBuffName)) return;
        _user.buffController.AddBuff(attachBuff);
    }

    void RemoveAttachBuff()
    {
        if (_user?.buffController == null) return;
        _user.buffController.TryOverBuff(attachBuff);
    }

    void TryAlignRow(int targetY)
    {
        var mc = _user.moveController;
        var stand = mc?.standTile;
        var map = MapManage.instance;
        if (stand == null || map == null) return;

        if (IsOnRow(_user, targetY))
        {
            _aligningRow = false;
            _targetRowY = targetY;
            // HorMove 的 nextTile 可能仍指向旧行，会把单位 Y 轴拉回去
            if (mc.nextTile != null && mc.nextTile.mapPos.y != targetY)
                RefreshHorMovePath();
            return;
        }

        if (_aligningRow && _targetRowY == targetY) return;

        int x = stand.mapPos.x;
        if (!map.IfInMapRange(x, targetY)) return;
        var targetTile = map.tiles[x, targetY];
        if (targetTile == null) return;

        if (_aligningRow && _targetRowY != targetY)
            mc.StopMove();

        mc.nextTile = null;
        _aligningRow = true;
        _targetRowY = targetY;
        _user.animatorController?.PlayMove();
        mc.MoveToTarget(targetTile, -1, OnRowAlignComplete);
    }

    void OnRowAlignComplete()
    {
        _aligningRow = false;
        RefreshHorMovePath();
    }

    void OnChessRemove(Chess chess)
    {
        _timer?.Stop();
        _timer = null;
        RemoveAttachBuff();
        if (_user != null)
            _user.OnRemove.RemoveListener(OnChessRemove);
        _user = null;
    }
}

/// <summary>
/// 攀附 Buff：场上有霸凌者时由 <see cref="PassiveSkill_Clinger"/> 维持，+100% 攻速（<see cref="Buff_BaseValueBuff_AttackSpeed.speed"/> = 1）。
/// </summary>
public class Buff_Zombie_ClingerAttach : Buff
{
    [SerializeReference] public Buff_BaseValueBuff_AttackSpeed attackSpeedBuff;
    [LabelText("攻速加成(相对基础)")]
    public float _attackSpeedBonus = 1f;

    public Buff_Zombie_ClingerAttach()
    {
        buffName = PassiveSkill_Clinger.AttachBuffName;
    }

    void EnsureBuffs()
    {
        if (attackSpeedBuff == null)
            attackSpeedBuff = new Buff_BaseValueBuff_AttackSpeed { speed = _attackSpeedBonus };
    }

    protected override void PrepareForRestore() => EnsureBuffs();

    public override Buff Clone()
    {
        var c = (Buff_Zombie_ClingerAttach)MemberwiseClone();
        c.attackSpeedBuff = attackSpeedBuff != null
            ? (Buff_BaseValueBuff_AttackSpeed)attackSpeedBuff.Clone()
            : null;
        return c;
    }

    public override void BuffEffect(Chess target)
    {
        EnsureBuffs();
        base.BuffEffect(target);
        attackSpeedBuff.target = target;
        attackSpeedBuff.BuffEffect(target);
    }

    public override void BuffOver()
    {
        if (attackSpeedBuff != null) attackSpeedBuff.BuffOver();
        base.BuffOver();
    }

    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        var other = resetBuff as Buff_Zombie_ClingerAttach;
        if (other?.attackSpeedBuff != null && attackSpeedBuff != null)
            attackSpeedBuff.BuffReset(other.attackSpeedBuff);
    }
}
