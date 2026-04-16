using System;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// AveMujica 乐队羁绊：种植带 tag 成员时施加 <see cref="Buff_AveMujica"/>；共享 fever 条，仅当 <b>任意成员释放技能</b>且 fever 已满（≥feverMax）时触发 Fever + 不屈；Fever 持续期间与结束后 15s 内 fever 不增长。
/// 羁绊检测用 Inspector 配置 <see cref="FetterDetectMode"/> / <see cref="FetterConfig"/>（Member 或 Tag）。
/// </summary>
public class AveMujica : Fetter
{
    public const string AveMujicaPlantTag = "AveMujica";

    [LabelText("Fever 上限")]
    public int feverMax = 450;

     
    public int feverCurrent;

    [SerializeReference]
    [Tooltip("种植时施加给 AveMujica 成员的 Buff")]
    public Buff_AveMujica buffTemplate;

    [SerializeReference]
    [Tooltip("Fever 触发时施加的不屈（建议 continueTime=20）")]
    public Buff_Unyielding unyieldingTemplate;

    [Tooltip("Fever 持续期间（秒），与不屈时长对齐；此期间 fever 不增长")]
    public float feverPhaseDuration = 20f;

    [Tooltip("Fever 结束后多少秒内 fever 不增长")]
    public float postFeverNoGainDuration = 15f;

    /// <summary>当前羁绊实例，供 <see cref="Buff_AveMujica"/> 上报 fever。</summary>
    public static AveMujica Instance { get; private set; }

    bool _feverActive;
    float _postFeverUnlockTime = float.NegativeInfinity;
    Timer _feverPhaseEndTimer;

    public override void FetterEffect(int count, int tier)
    {
        base.FetterEffect(count, tier);
        Instance = this;
        feverCurrent = 0;
        _feverActive = false;
        _postFeverUnlockTime = float.NegativeInfinity;
        EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), OnPlantChess);
    }

    public override void ResetFetter()
    {
        base.ResetFetter();
        EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), OnPlantChess);
        if (_feverPhaseEndTimer != null)
        {
            _feverPhaseEndTimer.Stop();
            _feverPhaseEndTimer = null;
        }
        if (Instance == this)
            Instance = null;
        _feverActive = false;
        feverCurrent = 0;
    }

    void OnPlantChess(Chess chess)
    {
        if (chess == null || buffTemplate == null)
            return;
        if (chess.propertyController?.creator?.plantTags == null ||
            !chess.propertyController.creator.plantTags.Contains(AveMujicaPlantTag))
            return;
        chess.buffController.AddBuff(buffTemplate);
    }

    /// <summary>外部（普攻/治疗/受击/若麦秒 tick 等）增加 fever，不会超过 feverMax，<b>不会</b>触发 Fever。</summary>
    public void TryAddFever(int delta)
    {
        if (delta <= 0)
            return;
        if (IsFeverGainBlocked())
            return;

        feverCurrent = Mathf.Min(feverCurrent + delta, feverMax);
    }

    const string NameUmi = "八幡海玲";

    /// <summary>
    /// 任意 AveMujica 成员释放技能时调用（由 <see cref="Buff_AveMujica"/> 绑定 <see cref="SkillController.onUseSkill"/>）。
    /// 八幡海玲：本次技能先 +10 fever（封顶）；若此时 fever ≥ feverMax 则清零并 <see cref="TriggerFever"/>。
    /// </summary>
    public void OnAveMujicaMemberUsedSkill(Chess user)
    {
        if (user == null)
            return;
        if (IsFeverGainBlocked())
            return;

        string name = user.propertyController?.creator?.chessName ?? "";
        if (name.Contains(NameUmi))
            TryAddFever(10);

        feverCurrent = Mathf.Min(feverCurrent, feverMax);
        if (feverCurrent < feverMax)
            return;

        feverCurrent = 0;
        TriggerFever();
    }

    bool IsFeverGainBlocked()
    {
        if (_feverActive)
            return true;
        if (TimerManage.GameTime < _postFeverUnlockTime)
            return true;
        return false;
    }

    void TriggerFever()
    {
        _feverActive = true;
        var tm = GameManage.instance?.timerManage;
        if (tm != null)
        {
            if (_feverPhaseEndTimer != null)
            {
                _feverPhaseEndTimer.Stop();
                _feverPhaseEndTimer = null;
            }
            _feverPhaseEndTimer = tm.AddTimer(OnFeverPhaseEnd, feverPhaseDuration, false);
        }
        else
        {
            _feverActive = false;
            _postFeverUnlockTime = TimerManage.GameTime + postFeverNoGainDuration;
        }

        var team = ChessTeamManage.Instance?.GetTeam("Player");
        if (team == null)
            return;

        for (int i = 0; i < team.Count; i++)
        {
            Chess c = team[i];
            if (c == null || c.IfDeath)
                continue;
            if (c.propertyController?.creator?.plantTags == null ||
                !c.propertyController.creator.plantTags.Contains(AveMujicaPlantTag))
                continue;

            TryEnterFeverState(c);
            if (unyieldingTemplate != null)
            {
                var ub = (Buff_Unyielding)unyieldingTemplate.Clone();
                ub.continueTime = feverPhaseDuration;
                c.buffController.AddBuff(ub);
            }
        }
    }

    void OnFeverPhaseEnd()
    {
        _feverPhaseEndTimer = null;
        _feverActive = false;
        _postFeverUnlockTime = TimerManage.GameTime + postFeverNoGainDuration;
    }

    static void TryEnterFeverState(Chess c)
    {
        try
        {
            c.stateController.ChangeState(StateName.FeverState);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[AveMujica] 无法切入 FeverState（请检查该棋子 StateGraph 是否含 FeverState）: {e.Message}");
        }
    }
}
