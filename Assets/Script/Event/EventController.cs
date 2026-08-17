using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using AsoulChess.Game.Core.Events;
using AsoulChess.Game.Core.Services;

public enum EventName
{
    WhenChessDestroy,
    CreateChess,
    WhenAttackTakeDamages,
    WhenBeAttack,
    WhenDeath,
    GameOver,
    RestartGame,
    GameStart,
    WhenSceneLoad,
    SelectState,
    WhenAddMember,
    CheckFetter,
    DamageFlatAmount,
    DamagePersentAmout,
    WhenHurt,
    AfterTakeDamage,
    WhenUseSkill,
    WhenSkillOver,
    WhenHealOther,
    WhenSummon,
    WhenEnterTile,
    WhenBuyChess,
    WhenSelectChess,
    TeamDeath,
    EnterNewRoom,
    WhenMoveTile,
    ChessEnterDesk,
    ChessLeaveDesk,
    ResetChess,
    WhenChessEnterWar,
    WhenSunLightChange,
    WaveZombieComming,
    LastWaveZombie,
    FirstZombieComming,
    GameSuccess,
    WhenPlantChess,
    WhenSweetChange,
    WhenLeaveLevel,
    PauseGame,
    ResumeGame,
    SnakeHitWall,
    SnakeEatFood,
    SnakeEatZombie,
    SnakeFoodSpawned,
    EnterMap,
}

public interface IEventAction { }

public class EventAction : IEventAction
{
    public UnityAction action;
}

public class EventAction<T> : IEventAction
{
    public UnityAction<T> action;
}

/// <summary>
/// AVZ facade over <see cref="GameServices.Events"/> (Core EventBus).
/// </summary>
public class EventController
{
    static EventController _instance;

    public static EventController Instance
    {
        get
        {
            if (_instance == null)
                _instance = new EventController();
            return _instance;
        }
    }

    EventBus Bus
    {
        get
        {
            if (GameServices.Events is EventBus bus)
                return bus;
            if (GameServices.Events == null)
                GameServices.RegisterDefaults();
            if (GameServices.Events is EventBus created)
                return created;
            var fallback = new EventBus();
            GameServices.Register(fallback);
            return fallback;
        }
    }

    public void AddListener(string name, UnityAction action) => Bus.AddListener(name, action);

    public void AddListener<T>(string name, UnityAction<T> action) => Bus.AddListener(name, action);

    public void RemoveListener(string name, UnityAction action) => Bus.RemoveListener(name, action);

    public void RemoveListener<T>(string name, UnityAction<T> action) => Bus.RemoveListener(name, action);

    public void TriggerEvent(string name) => Bus.Trigger(name);

    public void TriggerEvent<T>(string name, T message) => Bus.Trigger(name, message);
}
