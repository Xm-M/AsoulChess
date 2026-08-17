using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 芭菲猫被动：周期对自身 3×3 敌人施加减速（<see cref="ColdBuff"/>）；
/// 自身持有抹茶芭菲（默认 buffName「抹茶芭菲」）时改为冰冻（<see cref="FreezyBuff"/>）。
/// 常态关闭普攻。无技能专用 MonoBehaviour。
/// </summary>
[Serializable]
public class PassiveSkillEffect_ParfaitCat : ISkillEffect
{
    [Min(0.1f)]
    [Tooltip("脉冲间隔（TimerManage 关卡时间）")]
    public float intervalSeconds = 2f;

    [Min(0.1f)]
    [Tooltip("单次减速/冰冻持续秒数")]
    public float buffDurationSeconds = 5f;

    [Tooltip("与 MatchaParfaitBuff.buffName 一致")]
    public string matchaBuffName = "抹茶芭菲";

    [SerializeReference]
    [Tooltip("常态脉冲：减速")]
    public ColdBuff slowBuff;

    [SerializeReference]
    [Tooltip("抹茶芭菲期间脉冲：冰冻")]
    public FreezyBuff freezeBuff;

    [Tooltip("3×3 相对格；为空则运行时填满九宫")]
    public IGridFindTarget gridFindTarget;

    [Tooltip("每次脉冲在所在格生成的特效（减速/冰冻共用）；留空则不播")]
    public GameObject pulseEffect;

    Chess _user;
    Timer _timer;
    UnityAction<Chess> _onRemove;
    readonly List<Chess> _targets = new List<Chess>(16);

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null)
            return;

        Cleanup();
        _user = user;

        if (user.equipWeapon != null)
            user.equipWeapon.AttackAble = false;

        EnsureDefaults();

        _onRemove = OnUserRemove;
        user.OnRemove.AddListener(_onRemove);

        if (GameManage.instance?.timerManage == null)
            return;

        float interval = Mathf.Max(0.1f, intervalSeconds);
        OnPulse();
        _timer = GameManage.instance.timerManage.AddTimer(OnPulse, interval, true);
    }

    void EnsureDefaults()
    {
        if (slowBuff == null)
        {
            slowBuff = new ColdBuff
            {
                buffName = "冰冷",
                continueTime = buffDurationSeconds,
                _slowRate = -0.5f
            };
        }

        if (freezeBuff == null)
        {
            freezeBuff = new FreezyBuff
            {
                buffName = "冻结",
                continueTime = buffDurationSeconds,
                buff = new ColdBuff
                {
                    buffName = "冰冷",
                    continueTime = buffDurationSeconds,
                    _slowRate = -0.5f
                }
            };
        }

        if (gridFindTarget == null)
            gridFindTarget = new IGridFindTarget();

        if (gridFindTarget.relativeCells == null || gridFindTarget.relativeCells.Count == 0)
        {
            gridFindTarget.relativeCells = new List<Vector2Int>(9);
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    gridFindTarget.relativeCells.Add(new Vector2Int(x, y));
        }

        if (string.IsNullOrEmpty(matchaBuffName))
            matchaBuffName = "抹茶芭菲";
    }

    void OnPulse()
    {
        if (_user == null || _user.IfDeath || _user.buffController == null)
            return;

        EnsureDefaults();
        SpawnPulseEffect();

        gridFindTarget.FindTarget(_user, _targets);
        if (_targets.Count == 0)
            return;

        bool freezeMode = HasMatchaParfait();
        float duration = Mathf.Max(0.1f, buffDurationSeconds);

        for (int i = 0; i < _targets.Count; i++)
        {
            Chess enemy = _targets[i];
            if (enemy == null || enemy.IfDeath || enemy.buffController == null)
                continue;

            if (freezeMode)
                enemy.buffController.AddBuff(PrepareFreeze(duration));
            else
                enemy.buffController.AddBuff(PrepareSlow(duration));
        }
    }

    void SpawnPulseEffect()
    {
        if (pulseEffect == null || ObjectPool.instance == null)
            return;

        GameObject fx = ObjectPool.instance.Create(pulseEffect);
        if (fx == null)
            return;

        Vector3 pos = _user.moveController?.standTile != null
            ? _user.moveController.standTile.transform.position
            : _user.transform.position;
        fx.transform.position = pos;
    }

    bool HasMatchaParfait()
    {
        return _user.buffController.buffDic != null
               && _user.buffController.buffDic.ContainsKey(matchaBuffName);
    }

    ColdBuff PrepareSlow(float duration)
    {
        var buff = (ColdBuff)slowBuff.Clone();
        buff.continueTime = duration;
        return buff;
    }

    FreezyBuff PrepareFreeze(float duration)
    {
        var buff = (FreezyBuff)freezeBuff.Clone();
        buff.continueTime = duration;
        if (freezeBuff.buff != null)
        {
            buff.buff = (ColdBuff)freezeBuff.buff.Clone();
            buff.buff.continueTime = duration;
        }
        else if (buff.buff != null)
        {
            buff.buff = (ColdBuff)buff.buff.Clone();
            buff.buff.continueTime = duration;
        }
        return buff;
    }

    void OnUserRemove(Chess chess)
    {
        Cleanup();
    }

    void Cleanup()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer = null;
        }

        if (_user != null && _onRemove != null)
            _user.OnRemove.RemoveListener(_onRemove);

        _onRemove = null;
        _user = null;
        _targets.Clear();
    }
}
