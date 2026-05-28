using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 开局插件：监听 <see cref="EventName.WhenChessEnterWar"/>，对敌我所有入场单位施加攻速 Buff。
/// 读档恢复的植物不触发入场事件，因此在 <see cref="EventName.GameStart"/> 时再补一次全场单位。
/// </summary>
public class GameStartPlugin_ZombieFast : ILevelPlugin
{
    const string DefaultBuffName = "GameStart_ZombieFast";

    [LabelText("攻速 Buff（可空，用下面数值）")]
    [SerializeReference]
    public Buff_BaseValueBuff_AttackSpeed attackSpeedBuff;

    [LabelText("攻速加成（acceleRated + speed）")]
    public float attackSpeedBonus = 0.5f;

    public void StadgeEffect(LevelController levelController)
    {
        EventController.Instance.AddListener<Chess>(EventName.WhenChessEnterWar.ToString(), OnChessEnterWar);
        EventController.Instance.AddListener(EventName.GameStart.ToString(), OnGameStart);
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), OnLeaveLevel);
    }

    void OnGameStart()
    {
        ApplyBuffToAllFieldUnits();
    }

    void OnChessEnterWar(Chess chess)
    {
        ApplyBuff(chess);
    }

    void ApplyBuffToAllFieldUnits()
    {
        if (ChessTeamManage.Instance == null)
            return;

        ApplyBuffToTeam("Player");
        ApplyBuffToTeam("Enemy");
    }

    void ApplyBuffToTeam(string teamTag)
    {
        var team = ChessTeamManage.Instance.GetTeam(teamTag);
        if (team == null)
            return;

        for (int i = 0; i < team.Count; i++)
            ApplyBuff(team[i]);
    }

    void ApplyBuff(Chess chess)
    {
        if (chess == null || chess.IfDeath || chess.buffController == null)
            return;

        chess.buffController.AddBuff(GetBuffTemplate());
    }

    Buff_BaseValueBuff_AttackSpeed GetBuffTemplate()
    {
        if (attackSpeedBuff != null)
            return attackSpeedBuff;

        return new Buff_BaseValueBuff_AttackSpeed
        {
            buffName = DefaultBuffName,
            speed = attackSpeedBonus,
        };
    }

    void OnLeaveLevel()
    {
        EventController.Instance.RemoveListener<Chess>(EventName.WhenChessEnterWar.ToString(), OnChessEnterWar);
        EventController.Instance.RemoveListener(EventName.GameStart.ToString(), OnGameStart);
        EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), OnLeaveLevel);
    }

    public void OverPlugin(LevelController levelController)
    {
        OnLeaveLevel();
    }
}
