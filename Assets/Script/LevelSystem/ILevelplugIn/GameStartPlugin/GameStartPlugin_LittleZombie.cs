using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 开局插件：监听僵尸 <see cref="EventName.WhenChessEnterWar"/>，施加 <see cref="Buff_ChangeSize"/> 缩小体型。
/// </summary>
public class GameStartPlugin_LittleZombie : ILevelPlugin
{
    const string DefaultBuffName = "GameStart_LittleZombie";

    [LabelText("体型 Buff（可空，用下面 scale）")]
    [SerializeReference]
    public Buff_ChangeSize changeSizeBuff;

    [LabelText("localScale 目标值")]
    public Vector3 scale = new Vector3(0.7f, 0.7f, 0.7f);

    public void StadgeEffect(LevelController levelController)
    {
        EventController.Instance.AddListener<Chess>(EventName.WhenChessEnterWar.ToString(), OnEnemyEnterWar);
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), OnLeaveLevel);
    }

    void OnEnemyEnterWar(Chess chess)
    {
        if (chess == null || !chess.CompareTag("Enemy") || chess.IfDeath || chess.buffController == null)
            return;

        chess.buffController.AddBuff(GetBuffTemplate());
    }

    Buff_ChangeSize GetBuffTemplate()
    {
        if (changeSizeBuff != null)
            return changeSizeBuff;

        return new Buff_ChangeSize
        {
            buffName = DefaultBuffName,
            scale = scale,
        };
    }

    void OnLeaveLevel()
    {
        EventController.Instance.RemoveListener<Chess>(EventName.WhenChessEnterWar.ToString(), OnEnemyEnterWar);
        EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), OnLeaveLevel);
    }

    public void OverPlugin(LevelController levelController)
    {
        OnLeaveLevel();
    }
}
